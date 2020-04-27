using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.Threading;
using WaveEngine_MRTK_Demo.Emulation;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Media.SpeechRecognition;

namespace WaveEngine_MRTK_Demo.UWP.MixedReality
{
    class VoiceCommandService : Service, IVoiceCommandService
    {
        private string[] keywords;

        // The speech recognizer used throughout this sample.
        private SpeechRecognizer speechRecognizer;

        // Keep track of whether the continuous recognizer is currently running, so it can be cleaned up appropriately.
        private bool isListening;

        // Avoid initialization retries once if it failed the first time.
        private bool initializationFailed;

        private SemaphoreSlim startStopSemaphore;

        public event EventHandler<string> CommandRecognized;

        private void CustomInitialize()
        {
            this.startStopSemaphore = new SemaphoreSlim(1);

            this.InitializeRecognizer();

            CoreApplication.LeavingBackground += this.CoreApplication_LeavingBackground;
            CoreApplication.EnteredBackground += this.CoreApplication_EnteredBackground;
            CoreApplication.Resuming += this.CoreApplication_Resuming;
            CoreApplication.Suspending += this.CoreApplication_Suspending;
        }

        private void CoreApplication_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            Debug.WriteLine("VoiceCommand: LeavingBackground");
            this.InitializeRecognizer();
        }

        private async void CoreApplication_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Debug.WriteLine("VoiceCommand: EnteredBackground");
            await this.DeinitializeRecognizer();
        }

        private void CoreApplication_Resuming(object sender, object e)
        {
            Debug.WriteLine("VoiceCommand: Resuming");
            this.InitializeRecognizer();
        }

        private async void CoreApplication_Suspending(object sender, SuspendingEventArgs e)
        {
            Debug.WriteLine("VoiceCommand: Suspending");
            await this.DeinitializeRecognizer();
        }

        /// <summary>
        /// Initialize Speech Recognizer and compile constraints.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        private async void InitializeRecognizer()
        {
            if (this.initializationFailed)
            {
                Debug.WriteLine($"VoiceCommand: Initialization failed detected. Skipping InitializeRecognizer.");
                return;
            }

            await this.startStopSemaphore.WaitAsync();
            try
            {
                if (this.speechRecognizer != null)
                {
                    // cleanup prior to re-initializing this scenario.
                    this.speechRecognizer.StateChanged -= this.SpeechRecognizer_StateChanged;
                    this.speechRecognizer.ContinuousRecognitionSession.Completed -= this.ContinuousRecognitionSession_Completed;
                    this.speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= this.ContinuousRecognitionSession_ResultGenerated;
                    this.speechRecognizer.HypothesisGenerated -= this.SpeechRecognizer_HypothesisGenerated;

                    this.speechRecognizer.Dispose();
                    this.speechRecognizer = null;
                }

                this.speechRecognizer = new SpeechRecognizer();

                // Provide feedback to the user about the state of the recognizer. This can be used to provide visual feedback in the form
                // of an audio indicator to help the user understand whether they're being heard.
                this.speechRecognizer.StateChanged += this.SpeechRecognizer_StateChanged;

                // Apply the dictation topic constraint to optimize for dictated freeform speech.
                var listConstraint = new SpeechRecognitionListConstraint(this.keywords);
                this.speechRecognizer.Constraints.Add(listConstraint);
                SpeechRecognitionCompilationResult result = await this.speechRecognizer.CompileConstraintsAsync();
                if (result.Status == SpeechRecognitionResultStatus.Success)
                {
                    Debug.WriteLine("VoiceCommand: Grammar Compilation Success");
                }
                else
                {
                    Debug.WriteLine($"VoiceCommand: Grammar Compilation Failed: {result.Status}");
                }

                // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
                // some recognized phrases occur, or the garbage rule is hit. HypothesisGenerated fires during recognition, and
                // allows us to provide incremental feedback based on what the user's currently saying.
                this.speechRecognizer.ContinuousRecognitionSession.Completed += this.ContinuousRecognitionSession_Completed;
                this.speechRecognizer.ContinuousRecognitionSession.ResultGenerated += this.ContinuousRecognitionSession_ResultGenerated;
                this.speechRecognizer.HypothesisGenerated += this.SpeechRecognizer_HypothesisGenerated;

                await this.speechRecognizer.ContinuousRecognitionSession.StartAsync();
                this.isListening = true;
            }
            catch (Exception ex)
            {
                this.initializationFailed = true;
                Debug.WriteLine($"VoiceCommand: Exception raised during InitializeRecognizer: {ex}");
            }
            finally
            {
                this.startStopSemaphore.Release();
            }
        }

        private async Task DeinitializeRecognizer()
        {
            await this.startStopSemaphore.WaitAsync();
            try
            {
                if (this.speechRecognizer != null)
                {
                    if (this.isListening)
                    {
                        await this.speechRecognizer.ContinuousRecognitionSession.CancelAsync();
                        this.isListening = false;
                    }

                    this.speechRecognizer.ContinuousRecognitionSession.Completed -= this.ContinuousRecognitionSession_Completed;
                    this.speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= this.ContinuousRecognitionSession_ResultGenerated;
                    this.speechRecognizer.HypothesisGenerated -= this.SpeechRecognizer_HypothesisGenerated;
                    this.speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

                    this.speechRecognizer.Dispose();
                    this.speechRecognizer = null;
                }
            }
            finally
            {
                this.startStopSemaphore.Release();
            }
        }

        /// <summary>
        /// Provide feedback to the user based on whether the recognizer is receiving their voice input.
        /// </summary>
        /// <param name="sender">The recognizer that is currently running.</param>
        /// <param name="args">The current state of the recognizer.</param>
        private void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine($"VoiceCommand: Status changed {args.State}");
        }

        /// <summary>
        /// Handle events fired when a result is generated.
        /// </summary>
        /// <param name="sender">The Recognition session that generated this result</param>
        /// <param name="args">Details about the recognized speech</param>
        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            var confidence = args.Result.Confidence;
            var text = args.Result.Text;
            Debug.WriteLine($"VoiceCommand: Text recognized '{text}' with condifence {confidence}");

            // We may choose to discard content that has low confidence, as that could indicate that we're picking up
            // noise via the microphone, or someone could be talking out of earshot.
            if (confidence == SpeechRecognitionConfidence.Medium ||
                confidence == SpeechRecognitionConfidence.High)
            {
                WaveForegroundTask.Run(() => this.CommandRecognized?.Invoke(this, text));
            }
        }

        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            Debug.WriteLine($"VoiceCommand: RecognitionSession_Completed: {args.Status}");
        }

        private void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            Debug.WriteLine($"VoiceCommand: HypothesisGenerated: {args.Hypothesis.Text}");
        }

        public void ConfigureWords(string[] words)
        {
            this.keywords = words;
            this.CustomInitialize();
        }
    }
}

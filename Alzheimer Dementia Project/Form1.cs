using System;
using System.Threading.Tasks;
using System.Windows.Forms;


using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Alzheimer_Dementia_Project
{
    public partial class Form1 : Form
    {
        private static string APIKey = ""; // removed while uploading on github
        private SpeechRecognizer recognizer =
            new SpeechRecognizer(
                SpeechConfig.FromSubscription(APIKey, "eastus")
            );
        private SpeechRecognizer _recognizer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setSpeechRecognizerEventListeners();
            _recognizer = recognizer;
        }

        private void setSpeechRecognizerEventListeners()
        {
            recognizer.Recognized += setSpeechResult;

            recognizer.SessionStarted += Recognizer_SessionStarted;
            recognizer.SessionStopped += Recognizer_SessionStopped;
            recognizer.Canceled += Recognizer_Canceled;
        }

        private void Recognizer_Canceled(object sender, SpeechRecognitionCanceledEventArgs e)
        {

        }

        private void Recognizer_SessionStopped(object sender, SessionEventArgs e)
        {

            setProgressBarState(ProgressBarStyle.Blocks);

            button3.Enabled = false;
            button1.Enabled = true;
            button2.Enabled = true;

            recognizer = _recognizer;
        }

        private void Recognizer_SessionStarted(object sender, SessionEventArgs e)
        {

            setProgressBarState(ProgressBarStyle.Marquee);
        }

        public void setRecognitionResult(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(setRecognitionResult), new object[] { value });
                return;
            }
            richTextBox1.Text += value;
        }

        public void setProgressBarState(ProgressBarStyle progressBarStyle)
        {

            if (InvokeRequired)
            {
                this.Invoke(new Action<ProgressBarStyle>(setProgressBarState), new object[] { progressBarStyle });
                return;
            }
            progressBar1.Style = progressBarStyle;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = true;
            await recognizer.StartContinuousRecognitionAsync();
            button2.Enabled = true;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = true;

            String filePath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            DialogResult diagResult = openFileDialog.ShowDialog();
            if (diagResult != DialogResult.OK)
                return;
            filePath = openFileDialog.FileName;

            var config = SpeechConfig.FromSubscription(APIKey, "eastus");
            using (var audioInput = AudioConfig.FromWavFileInput(filePath))
            {
                recognizer = new SpeechRecognizer(config, audioInput);
                setSpeechRecognizerEventListeners();
                recognizer.Recognizing += setSpeechResult;
                await recognizer.StartContinuousRecognitionAsync();
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await RecognizeSpeechAsync();
            button3.Enabled = false;
            button1.Enabled = true;
            button2.Enabled = true;

            recognizer = _recognizer;
        }

        async Task RecognizeSpeechAsync()
        {
            await recognizer.StopContinuousRecognitionAsync();
        }

        private void setSpeechResult(object recog, SpeechRecognitionEventArgs _e)
        {
            var result = _e.Result;
            String resString = "";
            resString = getResultFromSpeechRecogResult(result);
            setRecognitionResult(resString);
        }

        private String getResultFromSpeechRecogResult(SpeechRecognitionResult result)
        {
            String resString = "";
            switch (result.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    resString = result.Text;
                    break;
                case ResultReason.NoMatch:
                    resString = "NOMATCH: Speech could not be recognized.";
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(result);
                    resString = $"CANCELED: Reason={cancellation.Reason}";

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        resString = ($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        resString += ($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        resString += ($"CANCELED: Did you update the subscription info?");
                    }
                    break;
            }
            return resString;
        }
    }
}

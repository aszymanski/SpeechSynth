using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SpeechSynth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SpeechSynthesizer synthesizer;
        private ResourceContext speechContext;
        private ResourceMap speechResourceMap;
        public MainPage()
        {
            this.InitializeComponent();
            synthesizer = new SpeechSynthesizer();

            speechContext = ResourceContext.GetForCurrentView();
            speechContext.Languages = new string[] { SpeechSynthesizer.DefaultVoice.Language };

            speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("LocalizationTTSResources");

            InitializeListboxVoiceChooser();
        }
        private async void Speak_Click(object sender, RoutedEventArgs e)
        {
            // If the media is playing, the user has pressed the button to stop the playback.
            if (media.CurrentState == MediaElementState.Playing)
            {
                media.Stop();
                btnSpeak.Content = "Speak";
            }
            else
            {
                // ComboBoxItem item = (ComboBoxItem)(listboxVoiceChooser.SelectedItem);
                // VoiceInformation voice = (VoiceInformation)(item.Tag);
                // synthesizer.Voice = voice;

                // update UI text to be an appropriate default translation.
                //speechContext.Languages = new string[] { voice.Language };
                // textToSynthesize.Text = speechResourceMap.GetValue("SynthesizeTextDefaultText", speechContext).ValueAsString;

                string text = textToSynthesize.Text;
                if (!String.IsNullOrEmpty(text))
                {
                    // Change the button label. You could also just disable the button if you don't want any user control.
                    btnSpeak.Content = "Stop";

                    try
                    {
                        // Create a stream from the text. This will be played using a media element.
                        SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(text);

                        // Set the source and start playing the synthesized audio stream.
                        media.AutoPlay = true;
                        media.SetSource(synthesisStream, synthesisStream.ContentType);
                        media.Play();
                    }
                    catch (FileNotFoundException)
                    {
                        // If media player components are unavailable, (eg, using a N SKU of windows), we won't
                        // be able to start media playback. Handle this gracefully
                        btnSpeak.Content = "Speak";
                        btnSpeak.IsEnabled = false;
                        textToSynthesize.IsEnabled = false;
                        listboxVoiceChooser.IsEnabled = false;
                        var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components unavailable");
                        await messageDialog.ShowAsync();
                    }
                    catch (Exception)
                    {
                        // If the text is unable to be synthesized, throw an error message to the user.
                        btnSpeak.Content = "Speak";
                        media.AutoPlay = false;
                        var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to synthesize text");
                        await messageDialog.ShowAsync();
                    }
                }
            }
        }
        void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            btnSpeak.Content = "Speak";
        }

        private void InitializeListboxVoiceChooser()
        {
            // Get all of the installed voices.
            var voices = SpeechSynthesizer.AllVoices;

            // Get the currently selected voice.
            VoiceInformation currentVoice = synthesizer.Voice;
            foreach (VoiceInformation voice in voices.OrderByDescending(p => p.Language))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Name = voice.DisplayName;
                item.Tag = voice;
                item.Content = voice.DisplayName + " (Language: " + voice.Language + ")";
                listboxVoiceChooser.Items.Add(item);
                Debug.WriteLine(voice.Language);
                // Check to see if we're looking at the current voice and set it as selected in the listbox.
                //currentVoice.Id[1].ToString() == 
                if (voice.DisplayName == "Microsoft Pablo Mobile")
                {
                    item.IsSelected = true;
                    listboxVoiceChooser.SelectedItem = item;
                }
            }
            //VoiceChange();
        }

        void VoiceChange()
        {
            try
            {
                ComboBoxItem item = (ComboBoxItem)(listboxVoiceChooser.Items[2]);
                VoiceInformation voice = (VoiceInformation)(item.Tag);
                synthesizer.Voice = voice;

                // update UI text to be an appropriate default translation.
                speechContext.Languages = new string[] { voice.Language };
                textToSynthesize.Text = speechResourceMap.GetValue("SynthesizeTextDefaultText", speechContext).ValueAsString;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex);
            }
        }
        private void change(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBoxItem item = (ComboBoxItem)(listboxVoiceChooser.SelectedItem);
                VoiceInformation voice = (VoiceInformation)(item.Tag);
                synthesizer.Voice = voice;

                // update UI text to be an appropriate default translation.
                speechContext.Languages = new string[] { voice.Language };
                textToSynthesize.Text = speechResourceMap.GetValue("SynthesizeTextDefaultText", speechContext).ValueAsString;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex);
            }

        }
    }
}

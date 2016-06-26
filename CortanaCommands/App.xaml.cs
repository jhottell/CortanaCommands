using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Media.SpeechRecognition;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;

namespace CortanaCommands
{
    /// Provides application-specific behavior to supplement the default Application class.
    sealed partial class App : Application
    {
        private MqttClient client;
        /// Initializes the singleton application object.  This is the first line of authored code
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// Invoked when the application is launched normally by the end user.  Other entry points
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();

            // Install the Voice Command Definition
            try
            {
                StorageFile vcdStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"CortanaCommands.xml");
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("There was an error registering the Voice Command Definitions", ex);
            }
        }

        /// Invoked when the application is activated by some means other than normal launching.
        protected async override void OnActivated(IActivatedEventArgs e)
        {
            // Handle when app is launched by Cortana
            if (e.Kind == ActivationKind.VoiceCommand)
            {
                VoiceCommandActivatedEventArgs commandArgs = e as VoiceCommandActivatedEventArgs;
                SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

                string voiceCommandName = speechRecognitionResult.RulePath[0];
                //string textSpoken = speechRecognitionResult.Text;
                //IReadOnlyList<string> recognizedVoiceCommandPhrases;

               // System.Diagnostics.Debug.WriteLine("voiceCommandName: " + voiceCommandName);
               // System.Diagnostics.Debug.WriteLine("textSpoken: " + textSpoken);

                //MessageDialog messageDialog = new MessageDialog("");

                switch (voiceCommandName)
                {
                    case "Activate_Alarm":
                        System.Diagnostics.Debug.WriteLine("Activate_Alarm command");
                        //messageDialog.Content = "Activate Alarm command";
                        break;

                    case "Open_Garage_Door":
                        SendMQTTMessage("Cortana/GarageDoor/", "OpenGarageDoor", true);
                        break;

                    case "Close_Garage_Door":
                        SendMQTTMessage("Cortana/GarageDoor/", "CloseGarageDoor", true);
                        break;

                    case "Dinner_Bell":
                        SendMQTTMessage("CortanaDoorBell", "Chime5", true);
                        break;

                    case "Door_Bell":
                        SendMQTTMessage("Cortana/DoorBell", "Chime1", true);

                        break;

                    default:
                       // messageDialog.Content = "Unknown command";
                        break;
                }

               // await messageDialog.ShowAsync();
            }
        }


        /// <summary>
        /// Sends MQTT messages, just pass the topic and payload
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        public void SendMQTTMessage(string topic, string message, bool close)
        {
            this.InitializeComponent();
            this.client = new MqttClient("iot.eclipse.org");
            //this.client.Connect(Guid.NewGuid().ToString(), "User", "Password"); // This is how you can use authentication!
            this.client.Connect(Guid.NewGuid().ToString());
            this.client.Publish(topic, Encoding.UTF8.GetBytes(message));
            if (close == true)
                { CoreApplication.Exit(); }
        }

        /// Invoked when Navigation to a certain page fails
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// Invoked when application execution is being suspended.  
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}

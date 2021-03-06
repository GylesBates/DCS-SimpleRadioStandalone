﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Common;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.UI.AwacsRadioOverlayWindow
{
    /// <summary>
    ///     Interaction logic for RadioControlGroup.xaml
    /// </summary>
    public partial class RadioControlGroup : UserControl
    {
        private const double MHz = 1000000;
        private bool _dragging;

        public RadioControlGroup()
        {
            InitializeComponent();

            RadioFrequency.MaxLines = 1;
            RadioFrequency.MaxLength = 7;

            RadioFrequency.LostFocus += RadioFrequencyOnLostFocus;

            RadioFrequency.KeyDown += RadioFrequencyOnKeyDown;

            RadioFrequency.GotFocus += RadioFrequencyOnGotFocus;

        }

        private void RadioFrequencyOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var dcsPlayerRadioInfo = RadioDCSSyncServer.DcsPlayerRadioInfo;

            if ((dcsPlayerRadioInfo == null) || !dcsPlayerRadioInfo.IsCurrent() ||
                RadioId > dcsPlayerRadioInfo.radios.Length - 1 || RadioId < 0)
            {
                //remove focus to somewhere else
                RadioVolume.Focus();
                Keyboard.ClearFocus(); //then clear altogether
            }
        }

        private
            void RadioFrequencyOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Enter)
            {
                //remove focus to somewhere else
                RadioVolume.Focus();
                Keyboard.ClearFocus(); //then clear altogher
            }
        }

        private void RadioFrequencyOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var dcsPlayerRadioInfo = RadioDCSSyncServer.DcsPlayerRadioInfo;

            if ((dcsPlayerRadioInfo != null) && dcsPlayerRadioInfo.IsCurrent() &&
                RadioId <= dcsPlayerRadioInfo.radios.Length - 1)
            {
                var currentRadio = dcsPlayerRadioInfo.radios[RadioId];

                if (currentRadio != null && currentRadio.freqMode == RadioInformation.FreqMode.OVERLAY)
                {
                    double freq = 0;
                    if (double.TryParse(RadioFrequency.Text.Trim(), out freq))
                    {
                        SendFrequencyChange(freq*MHz, false);
                    }
                    else
                    {
                        RadioFrequency.Text = "";
                    }

                   
                }
            }
        }

        public int RadioId { private get; set; }

        private void Up001_Click(object sender, RoutedEventArgs e)
        {
            SendFrequencyChange(MHz/100);
        }

        private void Up01_Click(object sender, RoutedEventArgs e)
        {
            SendFrequencyChange(MHz/10);
        }

        private void Up1_Click(object sender, RoutedEventArgs e)
        {
            SendFrequencyChange(MHz);
        }

        private void Up10_Click(object sender, RoutedEventArgs e)
        {
            SendFrequencyChange(MHz*10);
        }

        private void Down10_Click(object sender, RoutedEventArgs e)
        {
            SendFrequencyChange(MHz*-10);
        }

        private void Down1_Click(object sender, RoutedEventArgs e)
        {
            SendFrequencyChange(MHz*-1);
        }

        private void Down01_Click(object sender, RoutedEventArgs e)
        {
            SendFrequencyChange(MHz/10*-1);
        }

        private void Down001_Click(object sender, RoutedEventArgs e)
        {
            SendFrequencyChange(MHz/100*-1);
        }

        private void SendFrequencyChange(double frequency, bool delta = true)
        {

            var dcsPlayerRadioInfo = RadioDCSSyncServer.DcsPlayerRadioInfo;

            if ((dcsPlayerRadioInfo != null) && dcsPlayerRadioInfo.IsCurrent() &&
                RadioId < dcsPlayerRadioInfo.radios.Length && (RadioId >= 0))
            {
                var currentRadio = dcsPlayerRadioInfo.radios[RadioId];

                if (currentRadio != null && currentRadio.freqMode == RadioInformation.FreqMode.OVERLAY)
                {
                    //sort out the frequencies
                    var clientRadio = RadioDCSSyncServer.DcsPlayerRadioInfo.radios[RadioId];

                    if (delta)
                        clientRadio.freq += frequency;
                    else
                    {
                        clientRadio.freq = frequency;
                    }

                    //make sure we're not over or under a limit
                    if (clientRadio.freq > clientRadio.freqMax)
                    {
                        clientRadio.freq = clientRadio.freqMax;
                    }
                    else if (clientRadio.freq < clientRadio.freqMin)
                    {
                        clientRadio.freq = clientRadio.freqMin;
                    }

                    //make radio data stale to force resysnc
                    RadioDCSSyncServer.LastSent = 0;
                }
            }            
        }

        private void RadioSelectSwitch(object sender, RoutedEventArgs e)
        {
            var dcsPlayerRadioInfo = RadioDCSSyncServer.DcsPlayerRadioInfo;

            if ((dcsPlayerRadioInfo != null) && dcsPlayerRadioInfo.IsCurrent() &&
                RadioId < dcsPlayerRadioInfo.radios.Length && (RadioId >= 0))
            {
                var currentRadio = dcsPlayerRadioInfo.radios[RadioId];

                if (currentRadio.modulation != RadioInformation.Modulation.DISABLED
                    && RadioDCSSyncServer.DcsPlayerRadioInfo.control ==
                    DCSPlayerRadioInfo.RadioSwitchControls.HOTAS)
                {
                    RadioDCSSyncServer.DcsPlayerRadioInfo.selected = (short)RadioId;
                }
            }

        }

        private void RadioFrequencyText_Click(object sender, MouseButtonEventArgs e)
        {
            var dcsPlayerRadioInfo = RadioDCSSyncServer.DcsPlayerRadioInfo;

            if ((dcsPlayerRadioInfo != null) && dcsPlayerRadioInfo.IsCurrent() &&
                RadioId < dcsPlayerRadioInfo.radios.Length && (RadioId >= 0))
            {
                var currentRadio = dcsPlayerRadioInfo.radios[RadioId];

                if (currentRadio.modulation != RadioInformation.Modulation.DISABLED 
                    && RadioDCSSyncServer.DcsPlayerRadioInfo.control ==
                    DCSPlayerRadioInfo.RadioSwitchControls.HOTAS)
                {
                    RadioDCSSyncServer.DcsPlayerRadioInfo.selected = (short) RadioId;
                }
            }


        }

        private void RadioFrequencyText_RightClick(object sender, MouseButtonEventArgs e)
        {
            var currentRadio = RadioDCSSyncServer.DcsPlayerRadioInfo.radios[RadioId];

            if (currentRadio.freqMode == RadioInformation.FreqMode.OVERLAY)
            {
                //sort out the frequencies
                var clientRadio = RadioDCSSyncServer.DcsPlayerRadioInfo.radios[RadioId];
                if (clientRadio.secFreq > 0)
                {
                    clientRadio.secFreq = 0; // 0 indicates we want it overridden + disabled
                }
                else
                {
                    clientRadio.secFreq = 1; //indicates we want it back
                }

                //make radio data stale to force resysnc
                RadioDCSSyncServer.LastSent = 0;
            }
         
        }

        private void RadioVolume_DragStarted(object sender, RoutedEventArgs e)
        {
            _dragging = true;
        }


        private void RadioVolume_DragCompleted(object sender, RoutedEventArgs e)
        {
            var currentRadio = RadioDCSSyncServer.DcsPlayerRadioInfo.radios[RadioId];

            if (currentRadio.volMode == RadioInformation.VolumeMode.OVERLAY)
            {
                var clientRadio = RadioDCSSyncServer.DcsPlayerRadioInfo.radios[RadioId];

                clientRadio.volume = (float) RadioVolume.Value/100.0f;
            }

            _dragging = false;

        }

        private void ToggleButtons(bool enable)
        {
            if (enable)
            {
                Up10.Visibility = Visibility.Visible;
                Up1.Visibility = Visibility.Visible;
                Up01.Visibility = Visibility.Visible;
                Up001.Visibility = Visibility.Visible;

                Down10.Visibility = Visibility.Visible;
                Down1.Visibility = Visibility.Visible;
                Down01.Visibility = Visibility.Visible;
                Down001.Visibility = Visibility.Visible;

                Up10.IsEnabled = true;
                Up1.IsEnabled = true;
                Up01.IsEnabled = true;
                Up001.IsEnabled = true;

                Down10.IsEnabled = true;
                Down1.IsEnabled = true;
                Down01.IsEnabled = true;
                Down001.IsEnabled = true;
            }
            else
            {
                Up10.Visibility = Visibility.Hidden;
                Up1.Visibility = Visibility.Hidden;
                Up01.Visibility = Visibility.Hidden;
                Up001.Visibility = Visibility.Hidden;

                Down10.Visibility = Visibility.Hidden;
                Down1.Visibility = Visibility.Hidden;
                Down01.Visibility = Visibility.Hidden;
                Down001.Visibility = Visibility.Hidden;
            }
        }

        internal void RepaintRadioStatus()
        {
            SetupEncryption();

            var dcsPlayerRadioInfo = RadioDCSSyncServer.DcsPlayerRadioInfo;

            if ((dcsPlayerRadioInfo == null) || !dcsPlayerRadioInfo.IsCurrent() ||  RadioId > dcsPlayerRadioInfo.radios.Length - 1)
            {
                RadioActive.Fill = new SolidColorBrush(Colors.Red);
                RadioLabel.Text = "No Radio";
                RadioFrequency.Text = "Unknown";
               
                RadioMetaData.Text = "";

                RadioVolume.IsEnabled = false;

                ToggleButtons(false);

                //reset dragging just incase
                _dragging = false;
            }
            else
            {
                if (RadioId == dcsPlayerRadioInfo.selected)
                {
                    var transmitting = UdpVoiceHandler.RadioSendingState;

                    if (transmitting.IsSending && (transmitting.SendingOn == RadioId))
                    {
                        RadioActive.Fill = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#96FF6D"));
                    }
                    else
                    {
                        RadioActive.Fill = new SolidColorBrush(Colors.Green);
                    }
                }
                else
                {
                    RadioActive.Fill = new SolidColorBrush(Colors.Orange);
                }

                var currentRadio = dcsPlayerRadioInfo.radios[RadioId];

                if (currentRadio == null || currentRadio.modulation == RadioInformation.Modulation.DISABLED) // disabled
                {
                    RadioActive.Fill = new SolidColorBrush(Colors.Red);
                    RadioLabel.Text = "No Radio";
                    RadioFrequency.Text = "Unknown";
                    RadioMetaData.Text = "";
                 

                    RadioVolume.IsEnabled = false;

                    ToggleButtons(false);
                    return;
                }
                if (currentRadio.modulation == RadioInformation.Modulation.INTERCOM) //intercom
                {
                    RadioFrequency.Text = "INTERCOM";
                    RadioMetaData.Text = "";
                  
                }
                else
                {
                    if (!RadioFrequency.IsFocused 
                        || currentRadio.freqMode == RadioInformation.FreqMode.COCKPIT 
                        || currentRadio.modulation == RadioInformation.Modulation.DISABLED)
                    {
                        RadioFrequency.Text = (currentRadio.freq / MHz).ToString("0.000");
                    }

                    RadioMetaData.Text = (currentRadio.modulation == 0 ? "AM" : "FM");

                    if (currentRadio.secFreq > 100)
                    {
                        RadioMetaData.Text += " G";
                    }
                    if (currentRadio.enc && (currentRadio.encKey > 0))
                    {
                        RadioMetaData.Text += " E" + currentRadio.encKey; // ENCRYPTED
                    }
                }
                RadioLabel.Text = dcsPlayerRadioInfo.radios[RadioId].name;

                if (currentRadio.volMode == RadioInformation.VolumeMode.OVERLAY)
                {
                    RadioVolume.IsEnabled = true;

                    //reset dragging just incase
                    //    _dragging = false;
                }
                else
                {
                    RadioVolume.IsEnabled = false;

                    //reset dragging just incase
                    //  _dragging = false;
                }

                ToggleButtons(currentRadio.freqMode == RadioInformation.FreqMode.OVERLAY);

                if (_dragging == false)
                {
                    RadioVolume.Value = currentRadio.volume*100.0;
                }
            }
        }

        private void SetupEncryption()
        {
            var dcsPlayerRadioInfo = RadioDCSSyncServer.DcsPlayerRadioInfo;

            if (((dcsPlayerRadioInfo != null) || dcsPlayerRadioInfo.IsCurrent())
                && RadioId <= dcsPlayerRadioInfo.radios.Length - 1)
            {
            
                var currentRadio = dcsPlayerRadioInfo.radios[RadioId];

                if (currentRadio != null)
                {
                    EncryptionKeySpinner.Value = currentRadio.encKey;

                    //update stuff
                    if ((currentRadio.encMode == RadioInformation.EncryptionMode.NO_ENCRYPTION)
                        || (currentRadio.encMode == RadioInformation.EncryptionMode.ENCRYPTION_FULL)
                        || (currentRadio.modulation == RadioInformation.Modulation.INTERCOM))
                    {
                        //Disable everything
                        EncryptionKeySpinner.IsEnabled = false;
                        EncryptionButton.IsEnabled = false;
                        EncryptionButton.Content = "Enable";
                    }
                    else if (currentRadio.encMode ==
                             RadioInformation.EncryptionMode.ENCRYPTION_COCKPIT_TOGGLE_OVERLAY_CODE)
                    {
                        //allow spinner
                        EncryptionKeySpinner.IsEnabled = true;

                        //disallow encryption toggle
                        EncryptionButton.IsEnabled = false;
                        EncryptionButton.Content = "Enable";
                    }
                    else if (currentRadio.encMode ==
                             RadioInformation.EncryptionMode.ENCRYPTION_JUST_OVERLAY)
                    {
                        EncryptionKeySpinner.IsEnabled = true;
                        EncryptionButton.IsEnabled = true;

                        if (currentRadio.enc)
                        {
                            EncryptionButton.Content = "Disable";
                        }
                        else
                        {
                            EncryptionButton.Content = "Enable";
                        }
                    }
                }
                else
                {
                    //Disable everything
                    EncryptionKeySpinner.IsEnabled = false;
                    EncryptionButton.IsEnabled = false;
                    EncryptionButton.Content = "Enable";
                }


            }
            else
            {
                //Disable everything
                EncryptionKeySpinner.IsEnabled = false;
                EncryptionButton.IsEnabled = false;
                EncryptionButton.Content = "Enable";
            }
        }


        internal void RepaintRadioReceive()
        {
            var dcsPlayerRadioInfo = RadioDCSSyncServer.DcsPlayerRadioInfo;
            if (dcsPlayerRadioInfo == null)
            {
                RadioFrequency.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#00FF00"));
                RadioMetaData.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
            }
            else
            {
                var receiveState = UdpVoiceHandler.RadioReceivingState[RadioId];
                //check if current

                if ((receiveState == null) || !receiveState.IsReceiving())
                {
                    RadioFrequency.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#00FF00"));
                    RadioMetaData.Foreground =  new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
                }
                else if ((receiveState != null) && receiveState.IsReceiving())
                {
                    if (receiveState.IsSecondary)
                    {
                        RadioFrequency.Foreground = new SolidColorBrush(Colors.Red);
                        RadioMetaData.Foreground =  new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        RadioFrequency.Foreground = new SolidColorBrush(Colors.White);
                        RadioMetaData.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
                else
                {
                    RadioFrequency.Foreground =
                        new SolidColorBrush((Color) ColorConverter.ConvertFromString("#00FF00"));
                    RadioMetaData.Foreground =
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));
                }
            }
        }


        private void Encryption_ButtonClick(object sender, RoutedEventArgs e)
        {
            var dcsPlayerRadioInfo = RadioDCSSyncServer.DcsPlayerRadioInfo;

            if ((dcsPlayerRadioInfo != null) || dcsPlayerRadioInfo.IsCurrent())
            {
                var currentRadio = dcsPlayerRadioInfo.radios[RadioId];

                if (currentRadio.modulation != RadioInformation.Modulation.DISABLED) // disabled
                {
                    //update stuff
                    if (currentRadio.encMode == RadioInformation.EncryptionMode.ENCRYPTION_JUST_OVERLAY)
                    {
                        if (currentRadio.enc)
                        {
                            currentRadio.enc = false;
                            EncryptionButton.Content = "Enable";
                        }
                        else
                        {
                            currentRadio.enc = true;
                            EncryptionButton.Content = "Disable";
                        }

                        //make radio data stale to force resysnc
                        RadioDCSSyncServer.LastSent = 0;
                    }
                }
            }
        }

        private void EncryptionKeySpinner_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var dcsPlayerRadioInfo = RadioDCSSyncServer.DcsPlayerRadioInfo;

            if ((dcsPlayerRadioInfo != null) || dcsPlayerRadioInfo.IsCurrent())
            {
                var currentRadio = dcsPlayerRadioInfo.radios[RadioId];

                if (currentRadio.modulation != RadioInformation.Modulation.DISABLED) // disabled
                {
                    //update stuff
                    if ((currentRadio.encMode == RadioInformation.EncryptionMode.ENCRYPTION_COCKPIT_TOGGLE_OVERLAY_CODE) ||
                        (currentRadio.encMode == RadioInformation.EncryptionMode.ENCRYPTION_JUST_OVERLAY))
                    {
                        if (EncryptionKeySpinner.Value != null)
                        {
                            currentRadio.encKey = (byte) EncryptionKeySpinner.Value;
                            //make radio data stale to force resysnc
                            RadioDCSSyncServer.LastSent = 0;
                        }
                    }
                }
            }
        }
    }
}
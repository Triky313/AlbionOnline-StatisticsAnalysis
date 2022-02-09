using log4net;
using PacketDotNet;
using PhotonPackageParser;
using SharpPcap;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace StatisticsAnalysisTool.Network
{
    public static class NetworkManager
    {
        private static PhotonParser _receiver;
        private static MainWindowViewModel _mainWindowViewModel;
        private static readonly List<ICaptureDevice> CapturedDevices = new();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static bool IsNetworkCaptureRunning => CapturedDevices.Where(device => device.Started).Any(device => device.Started);

        public static bool StartNetworkCapture(MainWindowViewModel mainWindowViewModel, TrackingController trackingController)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _receiver = new AlbionPackageParser(trackingController, mainWindowViewModel);

            try
            {
                CapturedDevices.AddRange(CaptureDeviceList.Instance);
                return StartDeviceCapture();
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                _mainWindowViewModel.SetErrorBar(Visibility.Visible, LanguageController.Translation("PACKET_HANDLER_ERROR_MESSAGE"));
                _mainWindowViewModel.StopTracking();
                return false;
            }
        }

        private static bool StartDeviceCapture()
        {
            if (CapturedDevices.Count <= 0)
            {
                return false;
            }

            try
            {
                foreach (var device in CapturedDevices)
                {
                    PacketEvent(device);
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                _mainWindowViewModel.SetErrorBar(Visibility.Visible, LanguageController.Translation("PACKET_HANDLER_ERROR_MESSAGE"));
                _mainWindowViewModel.StopTracking();
                return false;
            }

            return true;
        }

        public static void StopNetworkCapture()
        {
            foreach (var device in CapturedDevices.Where(device => device.Started))
            {
                device.StopCapture();
                device.Close();
            }

            CapturedDevices.Clear();
        }

        private static void PacketEvent(ICaptureDevice device)
        {
            if (!device.Started)
            {
                device.Open(new DeviceConfiguration()
                {
                    Mode = DeviceModes.DataTransferUdp,
                    ReadTimeout = 5000
                });
                device.OnPacketArrival += Device_OnPacketArrival;
                device.StartCapture();
            }
        }

        private static void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data).Extract<UdpPacket>();
                if (packet != null && (packet.SourcePort == 5056 || packet.DestinationPort == 5056))
                {
                    _receiver.ReceivePacket(packet.PayloadData);
                }
            }
            catch (InvalidOperationException ioe)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ioe);
            }
            catch (OverflowException ex)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            }
            catch (Exception exc)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, exc);
                Log.Error(nameof(Device_OnPacketArrival), exc);
            }
        }
    }
}
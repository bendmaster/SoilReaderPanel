﻿using SoilReaderPanel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SoilReaderPanel.Models
{
    public class DeviceViewModel : Device
    {     
        [Display(Name = "Reading")]
        public string Reading { get; set; }
        public int ReadingAsInt { get; set; }
        public List<DeviceEvent>? DeviceEvents { get; set; }

        public DeviceViewModel(Device device, string latestReading, List<DeviceEvent>? deviceEvents)
        {
            base.DeviceID = device.DeviceID;
            base.ParticleDeviceID = device.ParticleDeviceID;
            base.DeviceName = device.DeviceName;
            base.ImageLocation = device.ImageLocation;
            Reading = latestReading;
            DeviceEvents = deviceEvents;
            try
            {
                ReadingAsInt = ((int)((double.Parse(latestReading) / 3200.0) * 100));
            }
            catch
            {
                ReadingAsInt = 0;
            }
        }
    }
}

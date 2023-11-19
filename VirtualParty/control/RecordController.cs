using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualParty.control
{
    public class RecordController
    {
        public WaveIn waveIn;
        public WaveFileWriter waveWriter;

        public void StartRecord(string filePath)
        {
            waveIn = new WaveIn();
            waveIn.DataAvailable += MWavIn_DataAvailable;
            waveWriter = new WaveFileWriter(filePath, waveIn.WaveFormat);
            waveIn.StartRecording();
        }

        public void StopRecord()
        {
            waveIn.StopRecording();
            waveIn.Dispose();
            waveWriter.Close();
        }

        private void MWavIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            int secondsRecorded = (int)waveWriter.Length / waveWriter.WaveFormat.AverageBytesPerSecond;
        }
    }
}

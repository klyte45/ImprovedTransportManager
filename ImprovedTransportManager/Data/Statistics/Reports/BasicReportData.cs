using System;
using static ImprovedTransportManager.Singleton.ITMTransportLineStatusesManager;

namespace ImprovedTransportManager.Data
{
    public abstract class BasicReportData
    {
        public long RefFrame { get; set; }

        public DateTime StartDate => SimulationManager.instance.FrameToTime((uint)RefFrame - OFFSET_FRAMES);
        public DateTime EndDate => SimulationManager.instance.FrameToTime((uint)RefFrame + FRAMES_PER_CYCLE_MASK - OFFSET_FRAMES);
        public float StartDayTime => FrameToDaytime(RefFrame - OFFSET_FRAMES);
        public float EndDayTime => FrameToDaytime(RefFrame + FRAMES_PER_CYCLE_MASK - OFFSET_FRAMES);

        private static float FrameToDaytime(long refFrame)
        {
            float num = (refFrame + DayTimeOffsetFrames) & (SimulationManager.DAYTIME_FRAMES - 1u);
            num *= SimulationManager.DAYTIME_FRAME_TO_HOUR;
            if (num >= 24f)
            {
                num -= 24f;
            }
            return num;
        }
    }
}
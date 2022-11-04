using ICities;
using Kwytto.Data;
using Kwytto.Utils;
using System;
using System.IO;
using static ImprovedTransportManager.Data.ITMTransportLineStatusesManager;

namespace ImprovedTransportManager.Data
{
    public abstract class TransportLineStorageBasicData : IDataExtension
    {
        public abstract string SaveId { get; }

        protected abstract Enum[] LoadOrder { get; }

        public IDataExtension Deserialize(Type type, byte[] rawData)
        {

            byte[] data;
            try
            {
                data = ZipUtils.UnzipBytes(rawData);
            }
            catch
            {
                LogUtils.DoLog("NOTE: Data is not zipped!");
                data = rawData;
            }
            var expectedSize = PredictSize(LoadOrder);

            int maxVehicles = (int)VehicleManager.instance.m_vehicles.m_size;

            if (expectedSize > data.Length)
            {
                LogUtils.DoWarnLog($"NOTE: Converting to fit in More Vehicles (expectedSize = {expectedSize} | length = {data.Length})");
                maxVehicles = 16384;
            }

            using (var s = new MemoryStream(data))
            {
                long version = ReadLong(s);
                foreach (Enum e in LoadOrder)
                {
                    if (version >= GetMinVersion(e))
                    {
                        var isVehicleData = IsVehicleEnum(e);
                        ITMTransportLineStatusesManager.instance.DoWithArray(e, (ref long[][] arrayRef) =>
                        {
                            int idx = GetIdxFor(e);

                            for (int i = 0; i < (isVehicleData ? maxVehicles : arrayRef.Length); i++)
                            {
                                arrayRef[i][idx] = DeserializeFunction(s);
                            }
                        }, (ref int[][] arrayRef) =>
                        {
                            int idx = GetIdxFor(e);

                            for (int i = 0; i < (isVehicleData ? maxVehicles : arrayRef.Length); i++)
                            {
                                arrayRef[i][idx] = (int)DeserializeFunction(s);
                            }
                        }, (ref ushort[][] arrayRef) =>
                        {
                            int idx = GetIdxFor(e);

                            for (int i = 0; i < (isVehicleData ? maxVehicles : arrayRef.Length); i++)
                            {
                                arrayRef[i][idx] = (ushort)DeserializeFunction(s);
                            }
                        });
                    }
                }
            }
            return this;
        }

        private bool IsVehicleEnum(Enum e)
        {
            switch (e)
            {
                case VehicleDataLong _:
                case VehicleDataSmallInt _:
                    return true;
            }
            return false;
        }

        public byte[] Serialize()
        {
            using (var s = new MemoryStream())
            {

                WriteLong(s, CURRENT_VERSION);
                foreach (Enum e in LoadOrder)
                {
                    ITMTransportLineStatusesManager.instance.DoWithArray(e, (ref long[][] arrayRef) =>
                    {
                        int idx = GetIdxFor(e);
                        for (int i = 0; i < arrayRef.Length; i++)
                        {
                            SerializeFunction(s, arrayRef[i][idx]);
                        }
                        LogUtils.DoWarnLog($"idxs= {arrayRef.Length};byte[] size: {s.Length} ({e.GetType()} {e})");
                    }, (ref int[][] arrayRef) =>
                    {
                        int idx = GetIdxFor(e);
                        for (int i = 0; i < arrayRef.Length; i++)
                        {
                            SerializeFunction(s, arrayRef[i][idx]);
                        }
                        LogUtils.DoWarnLog($"idxs= {arrayRef.Length};byte[] size: {s.Length} ({e.GetType()} {e})");
                    }, (ref ushort[][] arrayRef) =>
                    {
                        int idx = GetIdxFor(e);
                        for (int i = 0; i < arrayRef.Length; i++)
                        {
                            SerializeFunction(s, arrayRef[i][idx]);
                        }
                        LogUtils.DoWarnLog($"idxs= {arrayRef.Length}; byte[] size: {s.Length} ({e.GetType()} {e})");
                    });

                }
                return ZipUtils.ZipBytes(s.ToArray());
            }
        }
        protected static void WriteLong(Stream s, long value)
        {
            s.WriteByte((byte)((value >> 56) & 255L));
            s.WriteByte((byte)((value >> 48) & 255L));
            s.WriteByte((byte)((value >> 40) & 255L));
            s.WriteByte((byte)((value >> 32) & 255L));
            s.WriteByte((byte)((value >> 24) & 255L));
            s.WriteByte((byte)((value >> 16) & 255L));
            s.WriteByte((byte)((value >> 8) & 255L));
            s.WriteByte((byte)(value & 255L));
        }

        protected static long ReadLong(Stream s)
        {
            long num = (long)(s.ReadByte() & 255) << 56;
            num |= (long)(s.ReadByte() & 255) << 48;
            num |= (long)(s.ReadByte() & 255) << 40;
            num |= (long)(s.ReadByte() & 255) << 32;
            num |= (long)(s.ReadByte() & 255) << 24;
            num |= (long)(s.ReadByte() & 255) << 16;
            num |= (long)(s.ReadByte() & 255) << 8;
            return num | (s.ReadByte() & 255 & 255L);
        }
        protected static void WriteSemiLong(Stream s, long value)
        {
            s.WriteByte((byte)((value >> 40) & 255L));
            s.WriteByte((byte)((value >> 32) & 255L));
            s.WriteByte((byte)((value >> 24) & 255L));
            s.WriteByte((byte)((value >> 16) & 255L));
            s.WriteByte((byte)((value >> 8) & 255L));
            s.WriteByte((byte)(value & 255L));
        }

        protected static long ReadSemiLong(Stream s)
        {
            long num = (long)(s.ReadByte() & 255) << 40;
            num |= (long)(s.ReadByte() & 255) << 32;
            num |= (long)(s.ReadByte() & 255) << 24;
            num |= (long)(s.ReadByte() & 255) << 16;
            num |= (long)(s.ReadByte() & 255) << 8;
            return num | (s.ReadByte() & 255 & 255L);
        }
        protected static void WriteInt32(Stream s, long value)
        {
            s.WriteByte((byte)((value >> 24) & 255L));
            s.WriteByte((byte)((value >> 16) & 255L));
            s.WriteByte((byte)((value >> 8) & 255L));
            s.WriteByte((byte)(value & 255L));
        }

        protected static long ReadInt32(Stream s)
        {
            long num = (long)(s.ReadByte() & 255) << 24;
            num |= (long)(s.ReadByte() & 255) << 16;
            num |= (long)(s.ReadByte() & 255) << 8;
            return num | (s.ReadByte() & 255 & 255L);
        }
        protected static void WriteInt24(Stream s, long value)
        {
            s.WriteByte((byte)((value >> 16) & 255L));
            s.WriteByte((byte)((value >> 8) & 255L));
            s.WriteByte((byte)(value & 255L));
        }

        protected static long ReadInt24(Stream s)
        {
            long num = (long)(s.ReadByte() & 255) << 16;
            num |= (long)(s.ReadByte() & 255) << 8;
            return num | (s.ReadByte() & 255 & 255L);
        }
        protected static void WriteInt16(Stream s, long value)
        {
            s.WriteByte((byte)((value >> 8) & 255L));
            s.WriteByte((byte)(value & 255L));
        }

        protected static long ReadInt16(Stream s)
        {
            long num = (long)(s.ReadByte() & 255) << 8;
            return num | (s.ReadByte() & 255 & 255L);
        }

        protected virtual Action<Stream, long> SerializeFunction { get; } = WriteLong;
        protected virtual Func<Stream, long> DeserializeFunction { get; } = ReadLong;

        public bool IsLegacyCompatOnly => false;

        public void OnReleased() { }

        public void LoadDefaults(ISerializableData serializableData) { }
        #region Serialization Utils

        internal static int PredictSize(Enum[] enumArray)
        {
            int size = 0;
            foreach (var e in enumArray)
            {
                switch (e)
                {
                    case LineDataLong _:
                        size += 8 * TransportManager.MAX_LINE_COUNT;
                        break;
                    case VehicleDataLong _:
                        size += 8 * (int)VehicleManager.instance.m_vehicles.m_size;
                        break;
                    case StopDataLong _:
                        size += 8 * NetManager.MAX_NODE_COUNT;
                        break;
                    case LineDataSmallInt _:
                        size += 3 * TransportManager.MAX_LINE_COUNT;
                        break;
                    case VehicleDataSmallInt _:
                        size += 3 * (int)VehicleManager.instance.m_vehicles.m_size;
                        break;
                    case StopDataSmallInt _:
                        size += 3 * NetManager.MAX_NODE_COUNT;
                        break;
                    case LineDataUshort _:
                        size += 2 * TransportManager.MAX_LINE_COUNT;
                        break;
                }
            }
            return size;
        }

        internal delegate void DoWithArrayRef<T>(ref T[][] arrayRef) where T : struct, IComparable;

        internal static int GetIdxFor(Enum e)
        {
            switch (e)
            {
                case LineDataLong l:
                    return (int)l;
                case VehicleDataLong l:
                    return (int)l;
                case StopDataLong l:
                    return (int)l;
                case LineDataSmallInt l:
                    return (int)l;
                case VehicleDataSmallInt l:
                    return (int)l;
                case StopDataSmallInt l:
                    return (int)l;
                case LineDataUshort l:
                    return (int)l;
                default:
                    e.GetType();
                    throw new Exception("Invalid data in array deserialize!");
            }
        }

        private static int GetMinVersion(Enum e)
        {
            switch (e)
            {
                case LineDataLong l:
                    switch (l)
                    {
                        case LineDataLong.EXPENSE:
                        case LineDataLong.INCOME:
                            return 0;
                    }
                    break;
                case VehicleDataLong v:
                    switch (v)
                    {
                        case VehicleDataLong.EXPENSE:
                        case VehicleDataLong.INCOME:
                            return 0;
                    }
                    break;
                case StopDataLong s:
                    switch (s)
                    {
                        case StopDataLong.INCOME:
                            return 0;
                    }
                    break;
                case LineDataSmallInt l:
                    switch (l)
                    {
                        case LineDataSmallInt.TOTAL_PASSENGERS:
                        case LineDataSmallInt.TOURIST_PASSENGERS:
                        case LineDataSmallInt.STUDENT_PASSENGERS:
                            return 1;
                    }
                    break;
                case VehicleDataSmallInt v:
                    switch (v)
                    {
                        case VehicleDataSmallInt.TOTAL_PASSENGERS:
                        case VehicleDataSmallInt.TOURIST_PASSENGERS:
                        case VehicleDataSmallInt.STUDENT_PASSENGERS:
                            return 1;
                    }
                    break;
                case StopDataSmallInt s:
                    switch (s)
                    {
                        case StopDataSmallInt.TOTAL_PASSENGERS:
                        case StopDataSmallInt.TOURIST_PASSENGERS:
                        case StopDataSmallInt.STUDENT_PASSENGERS:
                            return 1;
                    }
                    break;
                case LineDataUshort l:
                    switch (l)
                    {
                        case LineDataUshort.W1_CHILD_MALE_PASSENGERS:
                        case LineDataUshort.W1_TEENS_MALE_PASSENGERS:
                        case LineDataUshort.W1_YOUNG_MALE_PASSENGERS:
                        case LineDataUshort.W1_ADULT_MALE_PASSENGERS:
                        case LineDataUshort.W1_ELDER_MALE_PASSENGERS:
                        case LineDataUshort.W2_CHILD_MALE_PASSENGERS:
                        case LineDataUshort.W2_TEENS_MALE_PASSENGERS:
                        case LineDataUshort.W2_YOUNG_MALE_PASSENGERS:
                        case LineDataUshort.W2_ADULT_MALE_PASSENGERS:
                        case LineDataUshort.W2_ELDER_MALE_PASSENGERS:
                        case LineDataUshort.W3_CHILD_MALE_PASSENGERS:
                        case LineDataUshort.W3_TEENS_MALE_PASSENGERS:
                        case LineDataUshort.W3_YOUNG_MALE_PASSENGERS:
                        case LineDataUshort.W3_ADULT_MALE_PASSENGERS:
                        case LineDataUshort.W3_ELDER_MALE_PASSENGERS:
                        case LineDataUshort.W1_CHILD_FEML_PASSENGERS:
                        case LineDataUshort.W1_TEENS_FEML_PASSENGERS:
                        case LineDataUshort.W1_YOUNG_FEML_PASSENGERS:
                        case LineDataUshort.W1_ADULT_FEML_PASSENGERS:
                        case LineDataUshort.W1_ELDER_FEML_PASSENGERS:
                        case LineDataUshort.W2_CHILD_FEML_PASSENGERS:
                        case LineDataUshort.W2_TEENS_FEML_PASSENGERS:
                        case LineDataUshort.W2_YOUNG_FEML_PASSENGERS:
                        case LineDataUshort.W2_ADULT_FEML_PASSENGERS:
                        case LineDataUshort.W2_ELDER_FEML_PASSENGERS:
                        case LineDataUshort.W3_CHILD_FEML_PASSENGERS:
                        case LineDataUshort.W3_TEENS_FEML_PASSENGERS:
                        case LineDataUshort.W3_YOUNG_FEML_PASSENGERS:
                        case LineDataUshort.W3_ADULT_FEML_PASSENGERS:
                        case LineDataUshort.W3_ELDER_FEML_PASSENGERS:
                            return 3;
                    }
                    break;
            }
            return 99999999;
        }

        #endregion
    }


}
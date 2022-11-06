using ColossalFramework;
using ColossalFramework.Math;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImprovedTransportManager.ModShared
{
    public sealed class BridgeCDFallback : IBridgeCD
    {
        public bool CustomDataAvailable => false;

        public int Priority => 1000;

        public bool IsBridgeEnabled => true;
        public Texture2D GetLineIcon(ushort lineId) => null;

        public void SetLineIcon(ushort lineId, Texture2D newIcon) { }

        public bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName)
        {
            GetNearestSegment(sidewalk, out Vector3 targetPosition, out float targetLength, out ushort targetSegmentId);
            if (targetSegmentId == 0)
            {
                number = 0;
                streetName = string.Empty;
                return false;
            }
            return GetAddressStreetAndNumber(targetPosition, targetSegmentId, targetLength, midPosBuilding, 0, out number, out streetName);
        }
        private static readonly ushort[] m_closestSegsFind = new ushort[64];
        private static void GetNearestSegment(Vector3 sidewalk, out Vector3 targetPosition, out float targetLength, out ushort targetSegmentId)
        {
            NetManager.instance.GetClosestSegments(sidewalk, m_closestSegsFind, out int found);
            targetPosition = default;
            targetLength = 0;
            targetSegmentId = 0;
            if (found == 0)
            {
                return;
            }
            else if (found > 1)
            {
                float minSqrDist = float.MaxValue;
                for (int i = 0; i < found; i++)
                {
                    ushort segId = m_closestSegsFind[i];
                    ref NetSegment seg = ref NetManager.instance.m_segments.m_buffer[segId];
                    if (!(seg.Info.GetAI() is RoadBaseAI))
                    {
                        continue;
                    }

                    GetClosestPositionAndDirectionAndPoint(seg, sidewalk, out Vector3 position, out _, out float length);
                    float sqrDist = Vector3.SqrMagnitude(sidewalk - position);
                    if (i == 0 || sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        targetPosition = position;
                        targetLength = length;
                        targetSegmentId = segId;
                    }
                }
            }
            else
            {
                targetSegmentId = m_closestSegsFind[0];
                GetClosestPositionAndDirectionAndPoint(NetManager.instance.m_segments.m_buffer[targetSegmentId], sidewalk, out targetPosition, out _, out targetLength);
            }
        }
        public static void GetClosestPositionAndDirectionAndPoint(NetSegment s, Vector3 point, out Vector3 pos, out Vector3 dir, out float length)
        {
            Bezier3 curve = default;
            curve.a = Singleton<NetManager>.instance.m_nodes.m_buffer[s.m_startNode].m_position;
            curve.d = Singleton<NetManager>.instance.m_nodes.m_buffer[s.m_endNode].m_position;
            bool smoothStart = (Singleton<NetManager>.instance.m_nodes.m_buffer[s.m_startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            bool smoothEnd = (Singleton<NetManager>.instance.m_nodes.m_buffer[s.m_endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            NetSegment.CalculateMiddlePoints(curve.a, s.m_startDirection, curve.d, s.m_endDirection, smoothStart, smoothEnd, out curve.b, out curve.c);
            float closestDistance = 1E+11f;
            float pointPerc = 0f;
            Vector3 targetA = curve.a;
            for (int i = 1; i <= 16; i++)
            {
                Vector3 vector = curve.Position(i / 16f);
                float dist = Segment3.DistanceSqr(targetA, vector, point, out float distSign);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    pointPerc = (i - 1f + distSign) / 16f;
                }
                targetA = vector;
            }
            float precision = 0.03125f;
            for (int j = 0; j < 4; j++)
            {
                Vector3 a2 = curve.Position(Mathf.Max(0f, pointPerc - precision));
                Vector3 vector2 = curve.Position(pointPerc);
                Vector3 b = curve.Position(Mathf.Min(1f, pointPerc + precision));
                float num6 = Segment3.DistanceSqr(a2, vector2, point, out float num7);
                float num8 = Segment3.DistanceSqr(vector2, b, point, out float num9);
                if (num6 < num8)
                {
                    pointPerc = Mathf.Max(0f, pointPerc - (precision * (1f - num7)));
                }
                else
                {
                    pointPerc = Mathf.Min(1f, pointPerc + (precision * num9));
                }
                precision *= 0.5f;
            }
            pos = curve.Position(pointPerc);
            dir = VectorUtils.NormalizeXZ(curve.Tangent(pointPerc));
            length = pointPerc;
        }
        private static bool GetAddressStreetAndNumber(Vector3 targetPosition, ushort targetSegmentId, float targetLength, Vector3 midPosBuilding, int metersOffset, out int number, out string streetName)
        {
            streetName = NetManager.instance.GetSegmentName(targetSegmentId)?.ToString();
            number = CalculateBuildingAddressNumber(targetPosition, targetSegmentId, targetLength, midPosBuilding, metersOffset);
            return true;
        }
        private static int CalculateBuildingAddressNumber(Vector3 targetPosition, ushort targetSegmentId, float targetLength, Vector3 midPosBuilding, int metersOffset)
        {
            int number;
            float angleTg = VectorUtils.XZ(targetPosition).GetAngleToPoint(VectorUtils.XZ(midPosBuilding));
            number = GetNumberAt(targetLength, targetSegmentId, metersOffset, out bool startAsEnd);
            if (angleTg == 90 || angleTg == 270)
            {
                angleTg += Math.Sign(targetPosition.z - midPosBuilding.z);
            }

            ref NetSegment targSeg = ref NetManager.instance.m_segments.m_buffer[targetSegmentId];
            Vector3 startSeg = NetManager.instance.m_nodes.m_buffer[targSeg.m_startNode].m_position;
            Vector3 endSeg = NetManager.instance.m_nodes.m_buffer[targSeg.m_endNode].m_position;

            float angleSeg = Vector2.zero.GetAngleToPoint(VectorUtils.XZ(endSeg - startSeg));
            if (angleSeg == 180)
            {
                angleSeg += Math.Sign(targetPosition.z - midPosBuilding.z);
            }


            //doLog($"angleTg = {angleTg};angleSeg = {angleSeg}");
            if ((angleTg + 90) % 360 < 180 ^ angleSeg < 180 ^ startAsEnd)
            {
                number &= ~1;
            }
            else
            {
                number |= 1;
            }

            return number;
        }

        private static int GetNumberAt(float targetLength, ushort targetSegmentId, int metersOffset, out bool startAsEnd, Vector3 cityCenter = default)
        {
            //doLog($"targets = S:{targetSegmentId} P:{targetPosition} D:{targetDirection.magnitude} L:{targetLength}");

            List<ushort> roadSegments = GetSegmentOrderRoad(targetSegmentId, false, false, false, out _, out _, out _);
            //doLog("roadSegments = [{0}] ", string.Join(",", roadSegments.Select(x => x.ToString()).ToArray()));
            int targetSegmentIdIdx = roadSegments.IndexOf(targetSegmentId);
            //doLog($"targSeg = {targetSegmentIdIdx} ({targetSegmentId})");
            ref NetSegment targSeg = ref NetManager.instance.m_segments.m_buffer[targetSegmentId];
            ref NetSegment preSeg = ref NetManager.instance.m_segments.m_buffer[0];
            ref NetSegment posSeg = ref NetManager.instance.m_segments.m_buffer[0];
            //doLog("PreSeg");
            if (targetSegmentIdIdx > 0)
            {
                preSeg = ref NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx - 1]];
            }
            //doLog("PosSeg");
            if (targetSegmentIdIdx < roadSegments.Count - 1)
            {
                posSeg = ref NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx + 1]];
            }
            //doLog("startAsEnd");
            startAsEnd = (targetSegmentIdIdx > 0 && (preSeg.m_endNode == targSeg.m_endNode || preSeg.m_startNode == targSeg.m_endNode)) || (targetSegmentIdIdx < roadSegments.Count && (posSeg.m_endNode == targSeg.m_startNode || posSeg.m_startNode == targSeg.m_startNode));

            //doLog($"startAsEnd = {startAsEnd}");

            if (startAsEnd)
            {
                targetLength = 1 - targetLength;
            }
            //doLog("streetName"); 

            //doLog("distanceFromStart");
            float distanceFromStart = 0;
            for (int i = 0; i < targetSegmentIdIdx; i++)
            {
                distanceFromStart += NetManager.instance.m_segments.m_buffer[roadSegments[i]].m_averageLength;
            }
            distanceFromStart += targetLength * targSeg.m_averageLength;
            return (int)Math.Round(distanceFromStart) + metersOffset;

            //doLog($"number = {number} B");
        }

        private static List<ushort> GetSegmentOrderRoad(ushort segmentID, bool requireSameDirection, bool requireSameSizeAndType, bool localAdjust, out bool startRef, out bool endRef, out ushort[] nodes)
        {
            NetSegment.Flags flags = NetManager.instance.m_segments.m_buffer[segmentID].m_flags;
            if (segmentID != 0 && flags != NetSegment.Flags.None)
            {
                List<ushort> path = CalculatePathNet(segmentID, false, requireSameDirection, requireSameSizeAndType, out nodes);
                path.Add(segmentID);

                ushort startNode0 = NetManager.instance.m_segments.m_buffer[path[0]].m_startNode;
                ushort endNode0 = NetManager.instance.m_segments.m_buffer[path[0]].m_endNode;
                ushort startNodeRef = NetManager.instance.m_segments.m_buffer[segmentID].m_startNode;
                ushort endNodeRef = NetManager.instance.m_segments.m_buffer[segmentID].m_endNode;

                bool circular = (path.Count > 2 && (startNode0 == endNodeRef || startNode0 == startNodeRef || endNode0 == endNodeRef || endNode0 == startNodeRef)) ||
                    (path.Count == 2 && (startNode0 == endNodeRef || startNode0 == startNodeRef) && (endNode0 == endNodeRef || endNode0 == startNodeRef));

                if (circular)
                {
                    LogUtils.DoLog("Circular!");
                    ushort refer = path.Min();
                    int referIdx = path.IndexOf(refer);
                    if (referIdx != 0)
                    {
                        path = path.GetRange(referIdx, path.Count - referIdx).Concat(path.Take(referIdx)).ToList();
                    }
                }
                else
                {
                    path.AddRange(CalculatePathNet(segmentID, true, requireSameDirection, requireSameSizeAndType, out _));
                }
                //doLog($"[s={strict}]path = [{string.Join(",", path.Select(x => x.ToString()).ToArray())}]");
                GetEdgeNodes(ref path, out startRef, out endRef, localAdjust);
                return path;
            }
            nodes = new ushort[0];
            startRef = false;
            endRef = false;
            return null;
        }


        private static List<ushort> CalculatePathNet(ushort segmentID, bool startSegment, bool requireSameDirection, bool requireSameSizeAndType, out ushort[] nodes)
        {
            var result = new List<ushort>();
            var resultNodes = new List<ushort>();
            ushort edgeNode = startSegment
                ? NetManager.instance.m_segments.m_buffer[segmentID].m_startNode
                : NetManager.instance.m_segments.m_buffer[segmentID].m_endNode;
            resultNodes.Add(edgeNode);
            CalculatePathNet(segmentID, segmentID, edgeNode, startSegment, ref result, ref resultNodes, requireSameDirection, requireSameSizeAndType);
            nodes = resultNodes.ToArray();
            return result;
        }


        private static void CalculatePathNet(ushort firstSegmentID, ushort segmentID, ushort nodeCurr, bool insertOnEnd, ref List<ushort> segmentOrder, ref List<ushort> nodeOrder, bool requireSameDirection, bool requireSameSizeAndType)
        {
            bool strict = requireSameDirection || requireSameSizeAndType;
            var possibilities = new List<ushort>();
            int crossingDirectionsCounter = 0;
            for (int i = 0; i < 8; i++)
            {
                ushort segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(i);

                if (segment != 0 && firstSegmentID != segment && segment != segmentID && !segmentOrder.Contains(segment) && IsSameName(segmentID, segment, false))
                {
                    crossingDirectionsCounter++;
                    if (strict && !IsSameName(segmentID, segment, requireSameDirection, requireSameSizeAndType))
                    {
                        continue;
                    }
                    possibilities.Add(segment);
                }
            }
            if (strict && crossingDirectionsCounter > 1)
            {
                return;
            }
            if (possibilities.Count > 0)
            {
                ushort segment = possibilities.Min();
                ushort otherEdgeNode = NetManager.instance.m_segments.m_buffer[segment].GetOtherNode(nodeCurr);
                if (insertOnEnd || segmentOrder.Count == 0)
                {
                    segmentOrder.Add(segment);
                    nodeOrder.Add(otherEdgeNode);
                }
                else
                {
                    segmentOrder.Insert(0, segment);
                    nodeOrder.Insert(0, otherEdgeNode);
                }
                CalculatePathNet(firstSegmentID, segment, otherEdgeNode, insertOnEnd, ref segmentOrder, ref nodeOrder, requireSameDirection, requireSameSizeAndType);
            }
        }

        private static List<ushort> GetCrossingPath(ushort relSegId)
        {
            ushort nodeCurr = NetManager.instance.m_segments.m_buffer[relSegId].m_startNode;
            var possibilities = new List<ushort>();
            for (int i = 0; i < 8; i++)
            {
                ushort segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(i);

                if (segment != 0 && relSegId != segment && !IsSameName(relSegId, segment, false))
                {
                    foreach (ushort prevSeg in possibilities)
                    {
                        if (IsSameName(prevSeg, segment, false))
                        {
                            goto LoopEnd;
                        }
                    }
                    possibilities.Add(segment);
                }
            LoopEnd:
                continue;
            }
            nodeCurr = NetManager.instance.m_segments.m_buffer[relSegId].GetOtherNode(nodeCurr);
            for (int i = 0; i < 8; i++)
            {
                ushort segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(i);

                if (segment != 0 && relSegId != segment && !IsSameName(relSegId, segment, false))
                {
                    foreach (ushort prevSeg in possibilities)
                    {
                        if (IsSameName(prevSeg, segment, false))
                        {
                            goto LoopEnd2;
                        }
                    }
                    possibilities.Add(segment);
                }
            LoopEnd2:
                continue;
            }
            return possibilities;
        }

        public static bool IsSameName(ushort segment1, ushort segment2, bool strict = false) => IsSameName(segment1, segment2, strict, strict);
        public static bool IsSameName(ushort segment1, ushort segment2, bool requireSameDirection, bool requireSameSizeAndHighway) => IsSameName(segment1, segment2, requireSameDirection, requireSameSizeAndHighway, requireSameSizeAndHighway);
        public static bool IsSameName(ushort segment1, ushort segment2, bool requireSameDirection, bool requireSameSize, bool requireSameHigwayFlag, bool requireSameDistrict = false, bool requireSameAI = false)
        {
            NetManager nm = NetManager.instance;
            NetInfo info = nm.m_segments.m_buffer[segment1].Info;
            NetInfo info2 = nm.m_segments.m_buffer[segment2].Info;
            if (info.m_class.m_service != info2.m_class.m_service)
            {
                return false;
            }
            if (info.m_class.m_subService != info2.m_class.m_subService)
            {
                return false;
            }
            ref NetSegment seg1 = ref nm.m_segments.m_buffer[segment1];
            ref NetSegment seg2 = ref nm.m_segments.m_buffer[segment2];
            if (!(seg1.Info.m_hasBackwardVehicleLanes && seg1.Info.m_hasForwardVehicleLanes) || !(seg2.Info.m_hasBackwardVehicleLanes && seg2.Info.m_hasForwardVehicleLanes))
            {
                if ((seg1.m_endNode == seg2.m_endNode || seg1.m_startNode == seg2.m_startNode))
                {
                    if (seg1.m_endNode == seg2.m_endNode && Mathf.Abs(seg1.m_endDirection.GetAngleXZ() - seg2.m_endDirection.GetAngleXZ()) < 90)
                    {
                        return false;
                    }
                    if (seg1.m_startNode == seg2.m_startNode && Mathf.Abs(seg1.m_startDirection.GetAngleXZ() - seg2.m_startDirection.GetAngleXZ()) < 90)
                    {
                        return false;
                    }

                    if ((nm.m_segments.m_buffer[segment1].m_flags & NetSegment.Flags.Invert) == (nm.m_segments.m_buffer[segment2].m_flags & NetSegment.Flags.Invert) && requireSameDirection)
                    {
                        return false;
                    }
                }
                if ((seg1.m_endNode == seg2.m_startNode || seg1.m_startNode == seg2.m_endNode))
                {
                    if (seg1.m_endNode == seg2.m_startNode && Mathf.Abs(seg1.m_endDirection.GetAngleXZ() - seg2.m_startDirection.GetAngleXZ()) < 90)
                    {
                        return false;
                    }
                    if (seg1.m_startNode == seg2.m_endNode && Mathf.Abs(seg1.m_startDirection.GetAngleXZ() - seg2.m_endDirection.GetAngleXZ()) < 90)
                    {
                        return false;
                    }
                    if ((nm.m_segments.m_buffer[segment1].m_flags & NetSegment.Flags.Invert) != (nm.m_segments.m_buffer[segment2].m_flags & NetSegment.Flags.Invert) && requireSameDirection)
                    {
                        return false;
                    }
                }
            }


            if (requireSameSize)
            {
                if (!((info.m_forwardVehicleLaneCount == info2.m_backwardVehicleLaneCount && info2.m_forwardVehicleLaneCount == info.m_backwardVehicleLaneCount)
                    || (info2.m_forwardVehicleLaneCount == info.m_forwardVehicleLaneCount && info2.m_backwardVehicleLaneCount == info.m_backwardVehicleLaneCount)))
                {
                    return false;
                }
            }
            if (requireSameAI)
            {
                if (info.GetAI().GetType() != info2.GetAI().GetType())
                {
                    return false;
                }
            }
            else if (requireSameHigwayFlag)
            {
                if ((info.GetAI() as RoadBaseAI)?.m_highwayRules != (info2.GetAI() as RoadBaseAI)?.m_highwayRules)
                {
                    return false;
                }
            }
            if (requireSameDistrict)
            {
                Vector3 pos1 = nm.m_segments.m_buffer[segment1].m_middlePosition;
                Vector3 pos2 = nm.m_segments.m_buffer[segment2].m_middlePosition;
                if (DistrictManager.instance.GetDistrict(pos1) != DistrictManager.instance.GetDistrict(pos2))
                {
                    return false;
                }
            }
            bool customName1 = (nm.m_segments.m_buffer[segment1].m_flags & NetSegment.Flags.CustomName) != NetSegment.Flags.None;
            bool customName2 = (nm.m_segments.m_buffer[segment2].m_flags & NetSegment.Flags.CustomName) != NetSegment.Flags.None;
            if (customName1 != customName2)
            {
                return false;
            }
            if (customName1)
            {
                InstanceID id = default;
                id.NetSegment = segment1;
                string name = Singleton<InstanceManager>.instance.GetName(id);
                id.NetSegment = segment2;
                string name2 = Singleton<InstanceManager>.instance.GetName(id);
                return name == name2;
            }
            ushort nameSeed = nm.m_segments.m_buffer[segment1].m_nameSeed;
            ushort nameSeed2 = nm.m_segments.m_buffer[segment2].m_nameSeed;
            return nameSeed == nameSeed2;
        }
        private static List<ushort> GetEdgeNodes(ref List<ushort> accessSegments, out bool nodeStartS, out bool nodeStartE, bool localAdjust)
        {
            if (accessSegments.Count > 1)
            {
                ref NetSegment seg0 = ref NetManager.instance.m_segments.m_buffer[accessSegments[0]];
                ref NetSegment seg1 = ref NetManager.instance.m_segments.m_buffer[accessSegments[1]];
                ref NetSegment segN2 = ref NetManager.instance.m_segments.m_buffer[accessSegments[accessSegments.Count - 2]];
                ref NetSegment segN1 = ref NetManager.instance.m_segments.m_buffer[accessSegments[accessSegments.Count - 1]];
                nodeStartS = seg1.m_startNode == seg0.m_endNode || seg1.m_endNode == seg0.m_endNode;
                nodeStartE = segN2.m_startNode == segN1.m_endNode || segN2.m_endNode == segN1.m_endNode;
                if (localAdjust)
                {
                    if (nodeStartS == ((NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) != 0))
                    {
                        accessSegments.Reverse();
                        bool temp = nodeStartS;
                        nodeStartS = nodeStartE;
                        nodeStartE = temp;
                    }
                }
                else
                {
                    ushort nodeF = nodeStartS ? seg0.m_startNode : seg0.m_endNode;
                    ushort nodeL = nodeStartE ? segN1.m_startNode : segN1.m_endNode;
                    if (VectorUtils.XZ(NetManager.instance.m_nodes.m_buffer[nodeF].m_position).GetAngleToPoint(VectorUtils.XZ(NetManager.instance.m_nodes.m_buffer[nodeL].m_position)) > 180)
                    {
                        accessSegments.Reverse();
                        bool temp = nodeStartS;
                        nodeStartS = nodeStartE;
                        nodeStartE = temp;
                    }
                }
            }
            else
            {
                nodeStartS = (NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) == 0;
                nodeStartE = (NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) != 0;
            }

            return accessSegments;
        }

    }
}

using ColossalFramework;
using ImprovedTransportManager.TransportSystems;
using Kwytto.Utils;
using System;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    internal class LineListItem : IDisposable
    {
        public uint m_lastUpdate;

        public InstanceID m_id;
        public TransportSystemType m_type;
        private Color m_cachedColor;
        public string LineName
        {
            get => TransportManager.instance.GetLineName(m_id.TransportLine);
            set => ModInstance.Controller.StartCoroutine(TransportManager.instance.SetLineName(m_id.TransportLine, value));
        }
        public Func<string> LineIdentifier { get; private set; }
        public Func<ushort> LineInternalSequentialNumber { get; private set; }
        public Func<bool> IsVisible { get; private set; }
        public Color LineColor
        {
            get => TransportManager.instance.GetLineColor(m_id.TransportLine);

            set
            {
                if (value.ToRGB() != m_uiTextureColor.GetPixel(0, 0).ToRGB())
                {
                    ModInstance.Controller.StartCoroutine(TransportManager.instance.SetLineColor(m_id.TransportLine, value));
                }
            }
        }

        public bool IsHovered { get; private set; }

        public int m_stopsCount;
        public uint m_passengersResCount;
        public uint m_passengersTouCount;
        public int m_budget;
        public int m_vehiclesCount;
        public int m_vehiclesTarget;
        public float m_lineFinancesBalance;
        public readonly Texture2D m_uiTextureColor = TextureUtils.New(1, 1);


        internal static LineListItem FromLine(ushort lineID)
        {
            TransportLine[] tlBuff = TransportManager.instance.m_lines.m_buffer;
            return new LineListItem
            {
                m_id = new InstanceID { TransportLine = lineID },
                m_type = TransportSystemTypeExtensions.FromLineId(lineID, false),
                LineIdentifier = () => tlBuff[lineID].m_lineNumber.ToString(),
                LineInternalSequentialNumber = () => tlBuff[lineID].m_lineNumber,
                IsVisible = () => (tlBuff[lineID].m_flags & TransportLine.Flags.Hidden) == 0
            };
        }

        public LineListItem GetUpdated()
        {
            if (SimulationManager.instance.m_currentTickIndex - 50 > m_lastUpdate)
            {
                if (m_cachedColor != LineColor)
                {
                    m_cachedColor = LineColor;
                    m_uiTextureColor.SetPixels(new[] { m_cachedColor });
                    m_uiTextureColor.Apply();
                }
                m_lastUpdate = SimulationManager.instance.m_currentTickIndex;
                ref TransportLine refLine = ref Singleton<TransportManager>.instance.m_lines.m_buffer[m_id.TransportLine];
                m_stopsCount = refLine.CountStops(m_id.TransportLine);
                m_passengersResCount = refLine.m_passengers.m_residentPassengers.m_averageCount;
                m_passengersTouCount = refLine.m_passengers.m_touristPassengers.m_averageCount;
                m_budget = Singleton<EconomyManager>.instance.GetBudget(refLine.Info.m_class); // ATUALIZAR COM VALOR CUSTOMIZADO!
                m_vehiclesCount = refLine.CountVehicles(m_id.TransportLine);
                m_vehiclesTarget = -1; //ATUALIZAR COM O CALCULO!
                m_lineFinancesBalance = 0f; //ATUALIZAR COM O SALDO!
            }
            return this;
        }

        public void Dispose()
        {
            OnMouseLeave();
            GameObject.Destroy(m_uiTextureColor);
        }

        public void OnMouseEnter()
        {
            if (!IsHovered)
            {
                IsHovered = true;
                Singleton<SimulationManager>.instance.AddAction(delegate
                {
                    if ((Singleton<TransportManager>.instance.m_lines.m_buffer[m_id.TransportLine].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                    {
                        Singleton<TransportManager>.instance.m_lines.m_buffer[m_id.TransportLine].m_flags |= TransportLine.Flags.Highlighted;
                    }
                });

            }
        }

        public void OnMouseLeave()
        {
            if (IsHovered)
            {
                IsHovered = false;
                Singleton<SimulationManager>.instance.AddAction(delegate
                {
                    if ((Singleton<TransportManager>.instance.m_lines.m_buffer[m_id.TransportLine].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None)
                    {
                        Singleton<TransportManager>.instance.m_lines.m_buffer[m_id.TransportLine].m_flags &= ~TransportLine.Flags.Highlighted;
                    }
                });

            }
        }


        public void ChangeLineVisibility(bool r)
        {
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                if (r)
                {
                    Singleton<TransportManager>.instance.m_lines.m_buffer[m_id.TransportLine].m_flags &= ~TransportLine.Flags.Hidden;
                }
                else
                {
                    Singleton<TransportManager>.instance.m_lines.m_buffer[m_id.TransportLine].m_flags |= TransportLine.Flags.Hidden;
                }
            });

        }
    }
}

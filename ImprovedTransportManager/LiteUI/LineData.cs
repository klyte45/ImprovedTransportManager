using ColossalFramework;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Utility;
using Kwytto.Utils;
using System;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    internal class LineData : IDisposable
    {
        public uint m_lastUpdate;

        public InstanceID m_id;
        public TransportSystemType m_type;
        private Color m_cachedColor;
        private LineActivityOptions m_lineActivity;

        public LineActivityOptions LineActivity
        {
            get => m_lineActivity;
            set => SimulationManager.instance.AddAction(() => TransportManager.instance.m_lines.m_buffer[m_id.TransportLine].SetActive((value & LineActivityOptions.Day) != 0, (value & LineActivityOptions.Night) != 0));
        }

        public string LineName
        {
            get => TransportManager.instance.GetLineName(m_id.TransportLine);
            set => SimulationManager.instance.AddAction(TransportManager.instance.SetLineName(m_id.TransportLine, value));
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
                    SimulationManager.instance.AddAction(TransportManager.instance.SetLineColor(m_id.TransportLine, value));
                }
            }
        }

        public bool IsHovered { get; private set; }
        public int FreeStops { get; private set; }
        public int BudgetEffectiveNow { get; private set; }
        public int BudgetEffectiveDay { get; private set; }
        public int BudgetEffectiveNight { get; private set; }
        public int BudgetCategoryNow { get; private set; }
        public int BudgetCategoryDay { get; private set; }
        public int BudgetCategoryNight { get; private set; }
        public int BudgetSelf
        {
            get => m_budgetSelf;
            set
            {
                m_budgetSelf = value;
                TransportManager.instance.m_lines.m_buffer[m_id.TransportLine].m_budget = (ushort)value;
            }
        }
        public int TicketPrice
        {
            get => m_ticketPrice;
            set
            {
                m_ticketPrice = value;
                TransportManager.instance.m_lines.m_buffer[m_id.TransportLine].m_ticketPrice = (ushort)value;
            }
        }
        public int VehiclesTargetNow { get; private set; }
        public int VehiclesTargetDay { get; private set; }
        public int VehiclesTargetNight { get; private set; }
        public int PassengersChild { get; private set; }
        public int PassengersTeen { get; private set; }
        public int PassengersYoung { get; private set; }
        public int PassengersAdult { get; private set; }
        public int PassengersSenior { get; private set; }
        public int PassengersCarOwning { get; private set; }
        public int TripsSaved { get; private set; }
        public bool Broken { get; private set; }
        public Texture2D LineIcon => null;

        public int m_stopsCount;
        public uint m_passengersResCount;
        public uint m_passengersTouCount;
        private int m_budgetSelf;
        private int m_ticketPrice;
        public int m_vehiclesCount;
        public float m_lineFinancesBalance;
        public float m_lengthKm;
        public readonly Texture2D m_uiTextureColor = TextureUtils.New(1, 1);


        internal static LineData FromLine(ushort lineID)
        {
            TransportLine[] tlBuff = TransportManager.instance.m_lines.m_buffer;
            return new LineData
            {
                m_id = new InstanceID { TransportLine = lineID },
                m_type = TransportSystemTypeExtensions.FromLineId(lineID, false),
                LineIdentifier = () => tlBuff[lineID].m_lineNumber.ToString(),
                LineInternalSequentialNumber = () => tlBuff[lineID].m_lineNumber,
                IsVisible = () => (tlBuff[lineID].m_flags & TransportLine.Flags.Hidden) == 0
            };
        }

        public LineData GetUpdated()
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


                PassengersChild = (int)refLine.m_passengers.m_childPassengers.m_averageCount;
                PassengersTeen = (int)refLine.m_passengers.m_teenPassengers.m_averageCount;
                PassengersYoung = (int)refLine.m_passengers.m_youngPassengers.m_averageCount;
                PassengersAdult = (int)refLine.m_passengers.m_adultPassengers.m_averageCount;
                PassengersSenior = (int)refLine.m_passengers.m_seniorPassengers.m_averageCount;
                PassengersCarOwning = (int)refLine.m_passengers.m_carOwningPassengers.m_averageCount;

                long probabilityUsingCar = 0;
                var totalCountPsg = m_passengersResCount + m_passengersTouCount;
                if (totalCountPsg != 0)
                {
                    probabilityUsingCar += PassengersTeen * 5;
                    probabilityUsingCar += PassengersYoung * (((15 * m_passengersResCount) + (20 * m_passengersTouCount) + (totalCountPsg >> 1)) / totalCountPsg);
                    probabilityUsingCar += PassengersAdult * (((20 * m_passengersResCount) + (20 * m_passengersTouCount) + (totalCountPsg >> 1)) / totalCountPsg);
                    probabilityUsingCar += PassengersSenior * (((10 * m_passengersResCount) + (20 * m_passengersTouCount) + (totalCountPsg >> 1)) / totalCountPsg);
                }
                if (probabilityUsingCar > 0)
                {
                    TripsSaved = Mathf.Clamp((int)(((PassengersCarOwning * 10000L) + (probabilityUsingCar >> 1)) / probabilityUsingCar), 0, 100);
                }
                else
                {
                    TripsSaved = 0;
                }

                m_budgetSelf = refLine.m_budget;
                m_ticketPrice = refLine.m_ticketPrice;
                BudgetCategoryNow = Singleton<EconomyManager>.instance.GetBudget(refLine.Info.m_class);
                BudgetCategoryDay = Singleton<EconomyManager>.instance.GetBudget(refLine.Info.m_class, false);
                BudgetCategoryNight = Singleton<EconomyManager>.instance.GetBudget(refLine.Info.m_class, true);
                BudgetEffectiveNow = m_budgetSelf * BudgetCategoryNow / 100;
                BudgetEffectiveDay = m_budgetSelf * BudgetCategoryDay / 100;
                BudgetEffectiveNight = m_budgetSelf * BudgetCategoryNight / 100;
                m_vehiclesCount = refLine.CountVehicles(m_id.TransportLine);
                VehiclesTargetNow = TEMP_CalculateTargetVehicles(BudgetEffectiveNow, refLine.m_totalLength, refLine.Info.m_defaultVehicleDistance);
                VehiclesTargetDay = TEMP_CalculateTargetVehicles(BudgetEffectiveDay, refLine.m_totalLength, refLine.Info.m_defaultVehicleDistance);
                VehiclesTargetNight = TEMP_CalculateTargetVehicles(BudgetEffectiveNight, refLine.m_totalLength, refLine.Info.m_defaultVehicleDistance);
                m_lengthKm = refLine.m_totalLength;
                m_lineFinancesBalance = 0f; //ATUALIZAR COM O SALDO!

                Broken = (refLine.m_flags & TransportLine.Flags.Complete) == 0;

                FreeStops = 0;
                var bufferN = NetManager.instance.m_nodes.m_buffer;
                for (int i = 0; i < ushort.MaxValue; i++)
                {
                    var nextStop = refLine.GetStop(i);
                    if (nextStop == 0)
                    {
                        break;
                    }
                    if (bufferN[nextStop].m_position.DistrictTariffMultiplierHere() == 0)
                    {
                        FreeStops++;
                    }
                }
                refLine.GetActive(out var day, out var night);
                m_lineActivity = (day ? LineActivityOptions.Day : 0) | (night ? LineActivityOptions.Night : 0);
            }
            return this;
        }

        private int TEMP_CalculateTargetVehicles(int budget, float lineLength, float defaultDistance)
        {
            return Mathf.CeilToInt(budget * lineLength / (defaultDistance * 100f));
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

        public void GoTo()
        {
            Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[Singleton<TransportManager>.instance.m_lines.m_buffer[m_id.TransportLine].m_stops].m_position;
            WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, m_id);
        }

        public void Delete()
        {
            ConfirmPanel.ShowModal("CONFIRM_LINEDELETE", (comp, ret) =>
            {
                if (ret == 1)
                {
                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        Singleton<TransportManager>.instance.ReleaseLine(m_id.TransportLine);
                    });
                }
            });
        }
    }
}

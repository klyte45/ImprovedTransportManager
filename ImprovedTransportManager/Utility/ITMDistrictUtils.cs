using ColossalFramework;
using UnityEngine;
namespace ImprovedTransportManager.Utility
{
    public static class ITMDistrictUtils
    {
        public static float DistrictTariffMultiplierHere(this Vector3 position, bool applyEffectInDistrict = false)
        {
            var instance = DistrictManager.instance;
            byte district = instance.GetDistrict(position);
            var servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
            var @event = instance.m_districts.m_buffer[(int)district].m_eventPolicies & Singleton<EventManager>.instance.GetEventPolicyMask();
            if ((servicePolicies & DistrictPolicies.Services.FreeTransport) != DistrictPolicies.Services.None)
            {
                if (applyEffectInDistrict)
                {
                    instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.FreeTransport;
                }
                return 0;
            }
            if ((@event & DistrictPolicies.Event.ComeOneComeAll) != DistrictPolicies.Event.None)
            {
                if (applyEffectInDistrict)
                {
                    instance.m_districts.m_buffer[district].m_eventPoliciesEffect |= DistrictPolicies.Event.ComeOneComeAll;
                }
                return 0;
            }
            if ((servicePolicies & DistrictPolicies.Services.HighTicketPrices) != DistrictPolicies.Services.None)
            {
                if (applyEffectInDistrict)
                {
                    instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.HighTicketPrices;
                }
                return 1.25f;
            }
            else
            {
                return 1;
            }
        }
    }
}

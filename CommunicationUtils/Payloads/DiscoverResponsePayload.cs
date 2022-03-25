using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class DiscoverResponsePayload:IPayload
    {
        public int? distanceFromCurrent { get; set; }
        public int? distanceN { get; set; }
        public int? distanceNE { get; set; }
        public int? distanceE { get; set; }
        public int? distanceSE { get; set; }
        public int? distanceS { get; set; }
        public int? distanceSW { get; set; }
        public int? distanceW { get; set; }
        public int? distanceNW { get; set; }

        public override string ToString()
        {
            return $"distanceFromCurrent {distanceFromCurrent}, distanceN {distanceN}," +
                $"distanceNE {distanceNE}, distanceE {distanceE} " +
                $"distanceSE {distanceSE}, distanceS {distanceS} " +
                $"distanceSW {distanceSW}, distanceW {distanceW} " +
                $"distanceNW {distanceNW}";
        }
    }
}

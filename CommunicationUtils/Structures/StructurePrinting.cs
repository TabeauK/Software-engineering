using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Structures
{
    public static class StructurePrinting
    {
        public static string GetName(this TeamColor team)
        {
            return Enum.GetName(typeof(TeamColor), team);
        }

        public static string GetName(this Direction team)
        {
            return Enum.GetName(typeof(Direction), team);
        }

        public static string GetName(this GoalAreaTileInformation team)
        {
            return Enum.GetName(typeof(GoalAreaTileInformation), team);
        }

        public static string GetName(this GoalAreaTileInformation[] team)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for(int i=0;i<team.Length;i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(team[i].GetName());
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static string GetName(this PickupPieceErrorType team)
        {
            return Enum.GetName(typeof(PickupPieceErrorType), team);
        }

        public static string GetName(this PlacePieceErrorType team)
        {
            return Enum.GetName(typeof(PlacePieceErrorType), team);
        }

        public static string GetName(this PlacePieceInfo team)
        {
            return Enum.GetName(typeof(PlacePieceInfo), team);
        }
    }
}

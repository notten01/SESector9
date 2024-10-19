namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class ExoTempest : IUnit
    {
        public bool SpaceAllowed => true;

        public bool AtmoAllowed => true;

        public int GameStage => 4;

        public string GetGridName(UnitType type)
        {
            switch (type)
            {
                case UnitType.Space:
                    return "RXhvIFRlbXBlc3Qgc3BhY2U=";

                case UnitType.Atmosphere:
                    return "RXhvIFRlbXBlc3QgYXRtbw==";

                default:
                    return "";
            }
        }
    }
}
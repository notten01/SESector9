namespace Sector9.Data.Scripts.Sector9.Server.HostileCommand.Units
{
    internal class AstralSovereign : IUnit
    {
        public bool SpaceAllowed => true;
        public bool AtmoAllowed => true;
        public int GameStage => 5;

        public string GetGridName(UnitType type)
        {
            return "QXN0cmFsIFNvdmVyZWlnbg==";
        }
    }
}

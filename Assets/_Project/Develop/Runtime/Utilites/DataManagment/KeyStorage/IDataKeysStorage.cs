namespace Assets._Project.Develop.Runtime.Utilites.DataManagment.KeyStorage
{
    public interface IDataKeysStorage
    {
        string GetKeyFor<Tdata>() where Tdata : ISaveData;
    }
}

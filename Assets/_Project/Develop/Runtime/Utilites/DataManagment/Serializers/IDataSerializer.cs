namespace Assets._Project.Develop.Runtime.Utilites.DataManagment.Serializers
{
    public interface IDataSerializer
    {
        string Serialize<TData>(TData data);
        TData Deserialize<TData>(string serializedData);
    }
}

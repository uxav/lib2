namespace UX.Lib2.DeviceSupport.Relays
{
    public interface IHoistController
    {
        void Up();
        void Down();
        void Stop();
    }
}
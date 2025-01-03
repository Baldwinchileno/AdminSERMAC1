namespace AdminSERMAC.Models
{
    public class Traspaso
    {
        public int Id { get; set; }
        public int SucursalOrigenId { get; set; }
        public int SucursalDestinoId { get; set; }
        public string Codigo { get; set; }
        public int Unidades { get; set; }
        public double Kilos { get; set; }
    }
}
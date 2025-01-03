namespace AdminSERMAC.Models
{
    public class Inventario
    {
        public string? Codigo { get; set; }
        public string? Producto { get; set; }
        public int Unidades { get; set; }
        public double Kilos { get; set; }
        public string? FechaMasAntigua { get; set; }
        public string? FechaMasNueva { get; set; }
    }
}
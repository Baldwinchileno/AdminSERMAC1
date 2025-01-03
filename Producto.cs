namespace AdminSERMAC.Models
{
    public class Producto
    {
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public int Unidades { get; set; }
        public double Kilos { get; set; }
        public string? FechaMasAntigua { get; set; }
        public string? FechaMasNueva { get; set; }
    }
}


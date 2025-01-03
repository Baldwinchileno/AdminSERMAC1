public class Venta
{
    public int NumeroGuia { get; set; }
    public string CodigoProducto { get; set; }
    public string Descripcion { get; set; }
    public int Bandejas { get; set; }
    public double KilosNeto { get; set; }
    public string FechaVenta { get; set; }
    public double Total { get; set; }
    public double Deuda { get; set; }
    public int PagadoConCredito { get; set; }
    public string RUT { get; set; }
    public string ClienteNombre { get; set; } // Nueva propiedad
}


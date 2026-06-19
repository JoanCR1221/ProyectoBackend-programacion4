namespace HackerRank1.DTO

{


    public class DonacionDTO
    {
        public int Id { get; set; }

        // Tipo de donación
        public string? Tipo { get; set; } // "dinero" o "especie"

        // Dinero
        public decimal? Monto { get; set; }

        // Especie
        public string? Descripcion { get; set; }
        public int? Cantidad { get; set; }
        public string? Unidad { get; set; }

        // Generales
        public string? Donante { get; set; }
        public DateTime? Fecha { get; set; }

        public bool Anulado { get; set; }


        public string? Banco { get; set; }
        public string? NumeroTransaccion { get; set; }


    }


}
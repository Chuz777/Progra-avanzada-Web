using System.ComponentModel.DataAnnotations;

namespace ConcesionarioN.Validations
{
    // HU-07/CA-02: año entre 1990 y el año actual + 1
    public class AnioUnidadAttribute : ValidationAttribute
    {
        public const int Minimo = 1990;
        public static int Maximo => DateTime.Now.Year + 1;

        public static string Mensaje => $"El año debe estar entre {Minimo} y {Maximo}.";

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is int anio && (anio < Minimo || anio > Maximo))
                return new ValidationResult(Mensaje);

            return ValidationResult.Success;
        }
    }
}

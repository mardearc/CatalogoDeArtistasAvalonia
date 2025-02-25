using Avalonia.Media.Imaging;

namespace CatalogoDeArtistas.Model;

public class Artista
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    public string Genero { get; set; }
    public string Nacionalidad { get; set; }
    public string CancionMasEscuchada { get; set; }
    public double Puntuacion { get; set; }
    public int Id { get; set; }
    public string FotoPath { get; set; } // Ruta de la foto en el sistema de archivos

    // Constructor actualizado
    public Artista(string nombre, int edad, string genero, string nacionalidad, string cancionMasEscuchada, double puntuacion, int id)
    {
        Nombre = nombre;
        Edad = edad;
        Genero = genero;
        Nacionalidad = nacionalidad;
        CancionMasEscuchada = cancionMasEscuchada;
        Puntuacion = puntuacion;
        Id = id;
    }
}

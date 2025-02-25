using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CatalogoDeArtistas.Model;

namespace CatalogoDeArtistas.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private readonly Bitmap _imagenPorDefecto; // Imagen por defecto
    private const string CsvFilePath = "artistas.csv"; // Ruta del archivo CSV
    private const string ImagesFolder = "Images"; // Carpeta para guardar las imágenes
    
    // Lista de géneros musicales disponibles
    public List<string> ListaGenerosMusicales { get; } = new List<string>
    {
        "Rock",
        "Pop",
        "Jazz",
        "Clásica",
        "Electrónica",
        "Reggaetón",
        "Hip-Hop",
        "Rap"
    };

    // Atributos de Artista
    [ObservableProperty] private string _nombre = "Desconocido";
    [ObservableProperty] private int _edad = 0;
    [ObservableProperty] private string _genero = "Género desconocido";
    [ObservableProperty] private string _nacionalidad = "Nacionalidad desconocida";
    [ObservableProperty] private string _cancionMasEscuchada = "Canción desconocida";
    [ObservableProperty] private double _puntuacion = 0;
    [ObservableProperty] private Bitmap _foto;
    [ObservableProperty] private string _fotoPath;
    
    // Property para artista seleccionado
    [ObservableProperty] private Artista _artistaSeleccionado;
    
    // Indice del artista actual
    [ObservableProperty] private int _indiceArtistaActual = -1;
    
    // Controlar la vista a través de enModoEdicion
    [ObservableProperty] private bool _enModoEdicion = false;
    
    // Saber si hay artista anterior y siguiente
    [ObservableProperty] private bool _hayAnterior = false;
    [ObservableProperty] private bool _haySiguiente = false;
    
    // Saber si se está modificando
    [ObservableProperty] private bool _enModoModificar = false;
    
    // Fotos del anterior y del siguiente
    [ObservableProperty] private Bitmap _fotoSiguiente;
    [ObservableProperty] private Bitmap _fotoAnterior;

    [ObservableProperty] private double _translateXActual = 0; // Imagen actual

    [ObservableProperty] private double _translateXSiguiente = 0; // Imagen siguiente (está fuera de la vista)

    [ObservableProperty] private double _translateXAnterior = 0; // Imagen anterior (fuera de la vista)

    [ObservableProperty] private double _opacityFotoAnterior = 1;
    
    [ObservableProperty] private double _opacityFotoSiguiente = 1;
    

    

    public ObservableCollection<Artista> ListaArtistas { get; } = new();

    public MainViewModel()
    {
        // Cargar la imagen por defecto
        _imagenPorDefecto = new Bitmap("Images/default.jpg");
        Foto = _imagenPorDefecto;
        FotoPath = "Images/default.jpg";

        // Crear la carpeta de imágenes si no existe
        if (!Directory.Exists(ImagesFolder))
        {
            Directory.CreateDirectory(ImagesFolder);
        }

        // Cargar artistas desde el archivo CSV al iniciar
        CargarArtistasDesdeCsv();
        MostrarPrimerArtista();
    }

    // Cargar artistas leyendo el CSV
    private void CargarArtistasDesdeCsv()
    {
        if (File.Exists(CsvFilePath))
        {
            var lineas = File.ReadAllLines(CsvFilePath);
            foreach (var linea in lineas)
            {
                var datos = linea.Split(',');
                if (datos.Length == 8)
                {
                    // Carga la imagen desde la ruta almacenada
                    Bitmap foto = _imagenPorDefecto;
                    if (!string.IsNullOrEmpty(datos[7]) && File.Exists(datos[7]))
                    {
                        foto = new Bitmap(datos[7]); // Carga la foto si la ruta es válida
                    }

                    var artista = new Artista(
                        datos[0],
                        int.Parse(datos[1]),
                        datos[2],
                        datos[3],
                        datos[4],
                        double.Parse(datos[5]),
                        int.Parse(datos[6])
                    )
                    {
                        FotoPath = datos[7] // Asigna la ruta de la foto
                    };
                    ListaArtistas.Add(artista);
                }
            }
        }
    }

    // Escribir csv
    private void GuardarArtistasEnCsv()
    {
        var lineas = ListaArtistas.Select(artista =>
        {
            string fotoPath = artista.FotoPath ?? ""; // Usa la ruta si está disponible, de lo contrario deja vacío
            return
                $"{artista.Nombre},{artista.Edad},{artista.Genero},{artista.Nacionalidad},{artista.CancionMasEscuchada},{artista.Puntuacion},{artista.Id},{fotoPath}";
        }).ToArray();

        File.WriteAllLines(CsvFilePath, lineas);
    }

    // Función para mostrar el primer artista al iniciar el programa
    [RelayCommand]
    private void MostrarPrimerArtista()
    {
        EnModoEdicion = false;
        if (ListaArtistas.Any())
        {
            IndiceArtistaActual = 0;
            MostrarArtistaActual();
        }
    }

    // Ver el artista anterior
    [RelayCommand]
    private async void AnteriorArtista()
    {
        if (IndiceArtistaActual > 0)
        {
            // Animamos el desplazamiento: la imagen actual va a la izquierda y la nueva entra desde la derecha
            for (double i = 0; i <= 300; i += 10)
            {
                TranslateXActual =+i; // Mueve la imagen actual hacia la derecha
                TranslateXSiguiente =+i; // Mueve la imagen siguiente al centro
                TranslateXAnterior =+i; // La imagen siguiente desaparece
                await Task.Delay(10); // Controla la velocidad de la animación
                
                //Efecto que se desvanece
                OpacityFotoSiguiente -= 0.2;
                if (TranslateXSiguiente >= 100)
                {
                    OpacityFotoSiguiente = 0;
                }
            }
            
            // Restar el índice actual 1 para mostrar el artista anterior
            IndiceArtistaActual--;
            MostrarArtistaActual();
            
            // Reiniciar posiciones de las fotos y opcacidad
            TranslateXActual = 0;
            TranslateXSiguiente = 0;
            TranslateXAnterior = 0;

            OpacityFotoSiguiente = 1;
        }
    }

    // Ver el artista siguiente
    [RelayCommand]
    private async void SiguienteArtista()
    {
        if (IndiceArtistaActual < ListaArtistas.Count - 1)
        {
            // Animamos el desplazamiento: la imagen actual va a la izquierda y la nueva entra desde la derecha
            for (double i = 0; i <= 300; i += 10)
            {
                TranslateXActual = -i - 20; // Mueve la imagen actual hacia la izquierda
                TranslateXSiguiente =  - i; // Mueve la imagen siguiente al centro
                TranslateXAnterior = - i; // La imagen anterior desaparece
                await Task.Delay(10); // Controla la velocidad de la animación

                //Efecto que se desvanece
                OpacityFotoAnterior -= 0.2;
                if (TranslateXAnterior <= -50)
                {
                    OpacityFotoAnterior = 0;
                }
            }

            // Cambiar al siguiente artista
            IndiceArtistaActual++;
            MostrarArtistaActual();

            // Reiniciar posiciones
            TranslateXActual = 0;
            TranslateXSiguiente = 0;
            TranslateXAnterior = 0;

            OpacityFotoAnterior = 1;
        }
    }

    


    // Función para mostrar los datos del artista que corresponde a través del índice actual
    private void MostrarArtistaActual()
    {
        var artista = ListaArtistas[IndiceArtistaActual];
        Nombre = artista.Nombre;
        Edad = artista.Edad;
        Genero = artista.Genero;
        Nacionalidad = artista.Nacionalidad;
        CancionMasEscuchada = artista.CancionMasEscuchada;
        Puntuacion = artista.Puntuacion;
        FotoPath = artista.FotoPath;
        Foto = new Bitmap(artista.FotoPath);

        MostrarFotos();
        
    }

    // Función para mostrar las fotos
    private void MostrarFotos()
    {
        // Solo se muestra la foto de anterior si existe
        if (IndiceArtistaActual > 0)
        {
            var anterior = ListaArtistas[IndiceArtistaActual - 1];
            FotoAnterior = new Bitmap(anterior.FotoPath);

            HayAnterior = true;
        }
        else
        {
            HayAnterior = false;
        }

        // Solo se muestra la foto del siguiente si existe
        if (IndiceArtistaActual < ListaArtistas.Count - 1)
        {
            var siguiente = ListaArtistas[IndiceArtistaActual + 1];
            FotoSiguiente = new Bitmap(siguiente.FotoPath);
            
            HaySiguiente = true;
        }
        else
        {
            HaySiguiente = false;
        }
        
        
        
    }

    // Al pulsar en agregar artista los datos se vacían y se activa el modo edición
    [RelayCommand]
    private void AgregarArtista()
    {
        EnModoEdicion = true;
        
        HayAnterior = false;
        HaySiguiente = false;
        
        Nombre = "";
        Edad = 0;
        Genero = "";
        Nacionalidad = "";
        CancionMasEscuchada = "";
        Puntuacion = 0;
        Foto = _imagenPorDefecto;
        FotoPath = "Images/default.jpg";
    }
    
    // Al pulsar en modificar se activa el modo modificar con los datos del artista del ínidce actual
    [RelayCommand]
    private void ModificarArtista()
    {
        EnModoEdicion = true;

        EnModoModificar = true;
        HayAnterior = false;
        HaySiguiente = false;
        
    }

    

    // Guardar el artista, al modificar o al agregar
    [RelayCommand]
    private void GuardarArtista()
    {
        if (EnModoModificar)
        {
            // Validación básica para asegurar que los campos obligatorios no estén vacíos
            if (string.IsNullOrWhiteSpace(Nombre) || Edad <= 0 || Foto == null)
            {
                // Si faltan datos obligatorios, no se guarda
                return;
            }

            // Crea un nuevo objeto Artista con los datos modificados
            var artistaModificado = new Artista(Nombre, Edad, Genero, Nacionalidad, CancionMasEscuchada, Puntuacion, IndiceArtistaActual + 1)
            {
                FotoPath = FotoPath // Asigna la ruta de la foto
            };

            // Actualiza el artista en la lista
            ListaArtistas[IndiceArtistaActual] = artistaModificado;

            // Guarda la lista actualizada en el archivo CSV
            GuardarArtistasEnCsv();

            // Salir del modo de edición y modificación
            EnModoEdicion = false;
            EnModoModificar = false;

            // Mostrar el artista actualizado
            MostrarArtistaActual();
        }
        else
        {
            // Validación básica
            if (string.IsNullOrWhiteSpace(Nombre) || Edad <= 0 || Foto == null)
            {
                // Si el nombre está vacío, la edad es inválida, o no hay foto seleccionada, no se guarda.
                return;
            }

            // Crear el nuevo artista con la información proporcionada
            var nuevoArtista = new Artista(Nombre, Edad, Genero, Nacionalidad, CancionMasEscuchada, Puntuacion,
                ListaArtistas.Count + 1)
            {
                FotoPath = FotoPath // Asigna la ruta de la foto
            };

            // Agregar a la lista
            ListaArtistas.Add(nuevoArtista);

            // Guardar la lista actualizada en el archivo CSV
            GuardarArtistasEnCsv();

            // Actualizar el estado de la vista
            EnModoEdicion = false;
            IndiceArtistaActual = ListaArtistas.Count - 1;
            MostrarArtistaActual();
        }
    }

    // Elimianr el artista con el indice actual
    [RelayCommand]
    private void EliminarArtista()
    {
        if (IndiceArtistaActual >= 0 && IndiceArtistaActual < ListaArtistas.Count)
        {
            ListaArtistas.RemoveAt(IndiceArtistaActual);
            if (ListaArtistas.Any())
            {
                IndiceArtistaActual = Math.Min(IndiceArtistaActual, ListaArtistas.Count - 1);
                MostrarArtistaActual();
            }
            else
            {
                IndiceArtistaActual = -1;
                Nombre = "Desconocido";
                Edad = 0;
                Genero = "Género desconocido";
                Nacionalidad = "Nacionalidad desconocida";
                CancionMasEscuchada = "Canción desconocida";
                Puntuacion = 0;
                Foto = _imagenPorDefecto;
                FotoPath = "Images/default.jpg";
            }

            // Guardar la lista actualizada en el archivo CSV
            GuardarArtistasEnCsv();
        }
    }

    // Función para subir la foto
    [RelayCommand]
    private async void SubirFoto(Window ventanaPadre)
    {
        var dlg = new OpenFileDialog();
        dlg.Filters.Add(new FileDialogFilter() { Name = "Imágenes JPEG", Extensions = { "jpg" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "Imágenes PNG", Extensions = { "png" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "Todos los archivos", Extensions = { "*" } });
        dlg.AllowMultiple = false;

        var result = await dlg.ShowAsync(ventanaPadre);
        if (result != null && result.Length > 0)
        {
            string rutaFotoOriginal = result[0];

            // Generar un nombre único para la foto
            string nombreFotoDestino = Guid.NewGuid().ToString() + Path.GetExtension(rutaFotoOriginal);
            string rutaFotoDestino = Path.Combine(ImagesFolder, nombreFotoDestino);

            // Copia la imagen a la carpeta destino
            File.Copy(rutaFotoOriginal, rutaFotoDestino, true); // El 'true' sobrescribe si ya existe

            // Asigna la ruta de la foto guardada en la propiedad FotoPath
            FotoPath = rutaFotoDestino;

            // Carga la imagen para mostrarla en la interfaz
            Foto = new Bitmap(rutaFotoDestino);
        }
    }
}
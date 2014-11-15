using System;
using System.Xml;
using System.Globalization;
using System.IO;
using Digi21.OpenGis.CoordinateSystems;
using Digi21.OpenGis.CoordinateTransformations;

namespace TransformaCoordenadasModeloOriAbsoluta
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Transforma las coordenadas modelo de un archivo .abs.xml de un SRC a otro. No transforma el cálculo, solo las coordenadas modelo.");

            if (args.Length < 4)
            {
                Console.Error.WriteLine("Error: No has proporcionado los parámetros suficientes.");
                Console.Error.WriteLine("[ruta del archivo .prj con la cadena WKT de las coordenadas modelo originales del archivo .abs.xml.]");
                Console.Error.WriteLine("[ruta del archivo .prj con la cadena WKT de las coordenadas modelo destino del archivo .abs.xml.]");
                Console.Error.WriteLine("[ruta del archivo .abs.xml a transformar]");
                Console.Error.WriteLine("[ruta del archivo .abs.xml a generar con las coordenadas transformadas]");
                Console.Error.WriteLine("");
                Console.Error.WriteLine("Ejemplo:");
                Console.Error.WriteLine(@"TransformaCoordenadasModeloOriAbsoluta E:\Tickets\435\wgs84-16n.prj E:\Tickets\435\wgs84.prj E:\Tickets\435\a.abs.xml E:\Tickets\435\b.abs.xml");
                return;
            }
            var fábricaSrc = new CoordinateSystemFactory();
            var srcOrigen = fábricaSrc.CreateFromWkt(File.ReadAllText(args[0]));
            var srcDestino = fábricaSrc.CreateFromWkt(File.ReadAllText(args[1]));
            var transformación = CoordinateTransformationFactory.CreateFromHorizontalCoordinateSystems(srcOrigen, srcDestino, null);

            var abs = new XmlDocument();
            abs.Load(args[2]);
            
            var nsManager = new XmlNamespaceManager(abs.NameTable);
            nsManager.AddNamespace("abs", "http://schemas.digi21.net/Digi3D/AbsoluteOrientation/v1.0");

            var coordenadasModelo = abs.SelectNodes("/abs:absolute/abs:points/abs:point/abs:model", nsManager);
            foreach (XmlNode coordenadaModelo in coordenadasModelo)
            {
                var nodoX = coordenadaModelo.SelectSingleNode("abs:x", nsManager);
                var nodoY = coordenadaModelo.SelectSingleNode("abs:y", nsManager);

                double[] coordenadasUtm = 
                {
                    double.Parse(nodoX.InnerText, CultureInfo.InvariantCulture),
                    double.Parse(nodoY.InnerText, CultureInfo.InvariantCulture),
                };

                var coordenadasGeo = transformación.MathTransform.Transform(coordenadasUtm);

                var clonX = nodoX.Clone();
                clonX.InnerText = coordenadasGeo[0].ToString(CultureInfo.InvariantCulture);
                coordenadaModelo.ReplaceChild(clonX, nodoX);

                var clonY = nodoY.Clone();
                clonY.InnerText = coordenadasGeo[1].ToString(CultureInfo.InvariantCulture);
                coordenadaModelo.ReplaceChild(clonY, nodoY);
            }

            abs.Save(args[3]);
        }
    }
}

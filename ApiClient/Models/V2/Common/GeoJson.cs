using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ApiClient.Models.V2.Common {
    /// <summary>
    /// TODO: WILL BE REPLACED BY LIBRARY!
    /// CoordinatesA1-A4 does deep casts and conversions from the 
    /// raw objects to collections of Coordinates of different arity. 
    /// Warning: very dynamic, will fail if you don't 
    /// actually know what type the Coordinate object is
    /// </summary>
    [KnownType(typeof (double[]))]
    [KnownType(typeof (double[][]))]
    [KnownType(typeof (double[][][]))]
    [KnownType(typeof (double[][][][]))]
    [KnownType(typeof (object[]))]
    public class GeoJson {
        public string Type { get; set; }
        public object Coordinates { get; set; }
        public List<GeoJson> Geometries { get; set; }

        /// <summary>
        /// For Point
        /// </summary>.
        public double[] CoordinatesA1 {
            get { return (double[]) Coordinates; }
        }

        /// <summary>
        /// For MultiPoint or LineString
        /// </summary>
        public IEnumerable<double[]> CoordinatesA2 {
            get {
                return ((object[]) Coordinates)
                    .Cast<object[]>()
                    .Select(os2 => os2
                        .Select(Convert.ToDouble)
                        .ToArray())
                    .ToArray();
            }
        }

        /// <summary>
        /// For MultiLineString or Polygon
        /// </summary>
        public IEnumerable<double[][]> CoordinatesA3 {
            get {
                return ((object[]) Coordinates)
                    .Cast<object[]>()
                    .Select(os => os
                        .Cast<object[]>()
                        .Select(os2 => os2
                            .Select(Convert.ToDouble)
                            .ToArray())
                        .ToArray());
            }
        }

        /// <summary>
        /// For GeometryCollection
        /// </summary>
        public IEnumerable<double[][][]> CoordinatesA4 {
            get {
                return ((object[]) Coordinates)
                    .Cast<object[]>()
                    .Select(os => os
                        .Cast<object[]>()
                        .Select(os2 => os2
                            .Cast<object[]>()
                            .Select(os3 => os3
                                .Select(Convert.ToDouble)
                                .ToArray())
                            .ToArray())
                        .ToArray());
            }
        }
    }
}
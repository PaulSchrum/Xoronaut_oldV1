﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
//using System.Drawing;


namespace DataTicker3D
{
    public class TimeTicker3D
    {
        public TimeTicker3D()
        {
            this.Brush = System.Windows.Media.Brushes.MistyRose;
            this.TickerWidth = 2.0;
            transform = new TimeTicker3Dtransform();
        }

        public GeometryModel3D TickerGeometryModel3D { get; protected set; }
        public Double TickerWidth { get; set; }
        public System.Windows.Media.Brush Brush { get; set; }
        public TimeTicker3Dtransform transform { get; set; }

        private SortedDictionary<Double, Double> rawData_;
        public SortedDictionary<Double, Double> rawData
        {
            get { return rawData_; }
            set
            {
                rawData_ = value;
                setupTransform();
                setupGeometryModel();
            }
        }

        private void setupGeometryModel()
        {
            MeshGeometry3D meshGeom = new MeshGeometry3D();
            foreach (var reading in rawData_)
            {
                //var deltaTime = reading.Key - transform.xStartDate;
                //Double x = deltaTime.TotalSeconds / (3600 * 24);
                Double x = reading.Key;
                Double y = (reading.Value - transform.yDatum) * transform.yExaggeration;
                Double z = transform.zAdjustment;
                meshGeom.Positions.Add(new Point3D(x, y, z - this.TickerWidth / 2));
                meshGeom.Positions.Add(new Point3D(x, y, z + this.TickerWidth / 2));
            }

            for (int i = 0; i < (rawData_.Count - 1) * 2; i += 2)
            {
                meshGeom.TriangleIndices.Add(i);
                meshGeom.TriangleIndices.Add(i + 1);
                meshGeom.TriangleIndices.Add(i + 2);

                meshGeom.TriangleIndices.Add(i + 1);
                meshGeom.TriangleIndices.Add(i + 3);
                meshGeom.TriangleIndices.Add(i + 2);
            }

            DiffuseMaterial material = new DiffuseMaterial();
            TickerGeometryModel3D = new GeometryModel3D();//(geometry, material);
            TickerGeometryModel3D.Geometry = meshGeom;
            TickerGeometryModel3D.Material = material;
            TickerGeometryModel3D.BackMaterial = material;
            //TickerGeometryModel3D.Transform = new Transform3DGroup();
        }

        private void setupTransform()
        {
            if (rawData == null) return;
            if (rawData.Count == 0) return;
            transform.startX = rawData_.FirstOrDefault().Key;
            transform.yDatum = (from reading in rawData_
                                select reading.Value).Min();
        }

    }

    public class TimeTicker3Dtransform
    {
        public Double startX { get; set; }
        public Double xExaggeration { get; set; }
        public Double yDatum { get; set; }
        public Double yExaggeration { get; set; }
        public Double zAdjustment { get; set; }

        public TimeTicker3Dtransform()
        {
            startX = 0.0;
            xExaggeration = 1.0;
            yDatum = 0.0;
            yExaggeration = 1.0;
            zAdjustment = 0.0;
        }

    }
}

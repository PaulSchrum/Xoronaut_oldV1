﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTicker3D
{
    public interface ITimeTickerDataProvider
    {
        SortedDictionary<Double, Double> getData();
        List<SortedDictionary<Double, Double>> getDataList();
    }
}

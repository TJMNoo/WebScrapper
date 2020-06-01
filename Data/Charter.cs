﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Enums;
using ChartJs.Blazor.ChartJS.Common.Handlers;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Time;
using ChartJs.Blazor.ChartJS.LineChart;
using ChartJs.Blazor.Charts;
using ChartJs.Blazor.Util;

namespace WebScraper.Data
{
    public class Charter
    {
        public LineConfig _lineConfig;
        public ChartJsLineChart _lineChartJs = new ChartJsLineChart();
        private LineDataset<TimeTuple<int>> _WeightDataSet;
        private GraphDummyData data = new GraphDummyData();
       
        public Charter(string keyword)
        {
            _lineConfig = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = keyword
                    },
                    Legend = new Legend
                    {
                        Display = false
                    },
                    Tooltips = new Tooltips
                    {
                        Mode = InteractionMode.Nearest,
                        Intersect = false
                    },
                    Scales = new Scales
                    {
                        yAxes = new List<CartesianAxis>
                    {
                        new LinearCartesianAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Position"
                            },
                            Ticks = new LinearCartesianTicks
                            {
                                Reverse = true,
                                Min = 1,
                                Max = 50
                            },

                        },
                    },
                        xAxes = new List<CartesianAxis>
                    {
                        new TimeAxis
                        {
                            Distribution = TimeDistribution.Linear,
                            Ticks = new TimeTicks
                            {
                                Source = TickSource.Data
                            },
                            Time = new TimeOptions
                            {
                                Unit = TimeMeasurement.Day,
                                Round = TimeMeasurement.Day,
                                TooltipFormat = "MM.DD.YYYY",
                                DisplayFormats = TimeDisplayFormats.DE_CH
                            },
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Date"
                            }
                        }
                    }
                    },
                    Hover = new LineOptionsHover
                    {
                        Intersect = true,
                        Mode = InteractionMode.Y
                    }
                }
            };

            _WeightDataSet = new LineDataset<TimeTuple<int>>
            {
                BackgroundColor = ColorUtil.FromDrawingColor(System.Drawing.Color.White),
                BorderColor = ColorUtil.FromDrawingColor(System.Drawing.Color.Red),
                Label = "Position",
                Fill = false,
                BorderWidth = 2,
                PointRadius = 2,
                PointBorderWidth = 2,
                SteppedLine = SteppedLine.False,
                Hidden = false
            };

            _WeightDataSet.RemoveAll(e => true);

            data.Shuffle();
            for (int i = 0; i < data.position.Count; i++)
            {
                var pos = data.position[i];
                var date = data.date[i];
                _WeightDataSet.Add(new TimeTuple<int>(new Moment(date), pos));
            }

            _lineConfig.Data.Datasets.Clear();
            _lineConfig.Data.Datasets.Add(_WeightDataSet);
            _lineChartJs.Update();
        }
    }
}

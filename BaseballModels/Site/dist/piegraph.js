"use strict";
var PieGraph = (function () {
    function PieGraph(element, points, title_text, colorscale) {
        if (colorscale === void 0) { colorscale = null; }
        var _this = this;
        this.points = points;
        if (colorscale !== null)
            this.colorscale = colorscale;
        else
            this.colorscale = [
                '#d43d51',
                '#df797d',
                '#e2acab',
                '#dddddd',
                '#a1c0b6',
                '#63a490',
                '#00876c'
            ];
        this.chart = new Chart(element, {
            type: 'pie',
            data: {
                labels: points.map(function (f) { return f.label; }),
                datasets: [{
                        label: '',
                        data: points.map(function (f) { return f.y; }),
                        backgroundColor: this.colorscale
                    }]
            },
            options: {
                plugins: {
                    legend: {
                        display: false,
                    },
                    title: {
                        display: true,
                        text: title_text,
                    },
                    tooltip: {
                        callbacks: {
                            label: function (item) {
                                var point = _this.points[item.dataIndex];
                                return "\t".concat((point.y * 100).toFixed(1), "%");
                            }
                        },
                    }
                },
                hover: {
                    mode: null,
                }
            }
        });
    }
    PieGraph.prototype.updateChart = function (values, title_text) {
        this.points.forEach(function (f, idx) { f.y = values[idx]; });
        this.chart.data.datasets[0].data = values;
        this.chart.options.plugins.title.text = title_text;
        this.chart.update();
    };
    return PieGraph;
}());

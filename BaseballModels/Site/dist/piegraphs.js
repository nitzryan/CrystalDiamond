"use strict";
var PieGraph = (function () {
    function PieGraph(element, points, title_text, colorscale) {
        if (colorscale === void 0) { colorscale = null; }
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
        this.chart = new this.chart(element, {
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
                    }
                },
                hover: {
                    mode: null,
                }
            }
        });
    }
    return PieGraph;
}());

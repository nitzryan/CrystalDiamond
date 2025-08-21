"use strict";
var LineGraph = (function () {
    function LineGraph(element, points, colorscale) {
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
            type: 'line',
            data: {
                labels: points.map(function (f) { return f.label; }),
                datasets: [{
                        label: 'Model 1',
                        data: points.map(function (f) { return f.y; })
                    }],
            },
            options: {
                scales: {
                    y: {
                        min: 0
                    }
                }
            }
        });
    }
    return LineGraph;
}());

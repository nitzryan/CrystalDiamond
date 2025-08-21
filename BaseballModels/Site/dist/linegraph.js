"use strict";
var POINT_DEFAULT_SIZE = 3;
var POINT_HIGHLIGHT_SIZE = 9;
var LineGraph = (function () {
    function LineGraph(element, points, callback, colorscale) {
        if (colorscale === void 0) { colorscale = null; }
        var _this = this;
        this.points = points;
        this.callback = callback;
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
                        data: points.map(function (f) { return f.y; }),
                        pointRadius: new Array(points.length).fill(POINT_DEFAULT_SIZE),
                    }],
            },
            options: {
                onClick: function (e, elements) {
                    var points = _this.chart.getElementsAtEventForMode(e, 'nearest', { intesect: false, axis: 'x' }, true);
                    if (points.length > 0) {
                        var firstPoint = points[0];
                        var index = firstPoint.index;
                        _this.highlight_index(index);
                        _this.callback(index);
                    }
                },
                scales: {
                    y: {
                        min: 0
                    }
                }
            }
        });
        if (points.length > 0)
            this.highlight_index(points.length - 1);
    }
    LineGraph.prototype.highlight_index = function (index) {
        var pointRadius = new Array(this.points.length).fill(POINT_DEFAULT_SIZE);
        pointRadius[index] = POINT_HIGHLIGHT_SIZE;
        this.chart.data.datasets[0].pointRadius = pointRadius;
        this.chart.update();
    };
    return LineGraph;
}());

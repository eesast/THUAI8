var canvas = document.getElementById("map");
var ctx = canvas.getContext("2d");
var currentColor = 0;
var map = new Array(50);
for (var i = 0; i < 50; i++) {
    map[i] = new Array(50);
    for (var j = 0; j < 50; j++) {
        map[i][j] = 0;
    }
}
var color = [
    "#ED1C24", // Home
    "#FFFFFF", // Space
    "#B97A57", // Barrier
    "#22B14C", // Bush
    "#A349A4", // E_Resource
    "#99D9EA", // A_Resource
    "#FF7F27", // Construction
];

const placeType = {
    Home: 1,
    Space: 2,
    Barrier: 3,
    Bush: 4,
    E_Resource: 5,
    A_Resource: 6,
    Construction: 7,
};

function draw() {
    ctx.clearRect(0, 0, 500, 500);
    for (var i = 0; i < 50; i++) {
        for (var j = 0; j < 50; j++) {
            ctx.fillStyle = color[map[i][j]-1];
            ctx.fillRect(i * 10, j * 10, 10, 10);
        }
    }
}

canvas.onmousedown = function (e) {
    var x = Math.floor(e.offsetX / 10);
    var y = Math.floor(e.offsetY / 10);
    map[x][y] = currentColor;
    draw();
}

document.getElementById("space").onclick = function () {
    currentColor = 2;
    document.getElementById("current").innerHTML = "当前：Space";
}
document.getElementById("barrier").onclick = function () {
    currentColor = 3;
    document.getElementById("current").innerHTML = "当前：Barrier";
}
document.getElementById("bush").onclick = function () {
    currentColor = 4;
    document.getElementById("current").innerHTML = "当前：Bush";
}
document.getElementById("Aresource").onclick = function () {
    currentColor = 6;
    document.getElementById("current").innerHTML = "当前：A_Resource";
}
document.getElementById("Eresource").onclick = function () {
    currentColor = 5;
    document.getElementById("current").innerHTML = "当前：E_Resource";
}
document.getElementById("construction").onclick = function () {
    currentColor = 7;
    document.getElementById("current").innerHTML = "当前：Construction";
}
document.getElementById("home").onclick = function () {
    currentColor = 1;
    document.getElementById("current").innerHTML = "当前：Home";
}

function saveAsTxt() {
    var str = "";
    for (var i = 0; i < 50; i++) {
        for (var j = 0; j < 50; j++) {
            str += map[j][i];
            str += " ";
        }
        str += "\n";
    }
    var a = document.createElement("a");
    a.style.display = "none";
    a.href = "data:text/plain;charset=utf-8," + str;
    a.download = "map.txt";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

function saveAsCs() {
    var str = "public static uint[,] Map = new uint[50, 50] {\n";
    for (var i = 0; i < 50; i++) {
        str += "    {";
        for (var j = 0; j < 50; j++) {
            str += map[j][i];
            if (j != 49) {
                str += ", ";
            }
        }
        str += "}";
        if (i != 49) {
            str += ",";
        }
        str += "\n";
    }
    str += "};";
    var a = document.createElement("a");
    a.style.display = "none";
    a.href = "data:text/plain;charset=utf-8," + str;
    a.download = "map.cs";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

function saveAsPng() {
    var a = document.createElement("a");
    a.style.display = "none";
    a.href = canvas.toDataURL("image/png");
    a.download = "map.png";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

function saveAsJson() {
    var rows = [];
    for (var i = 0; i < 50; i++) {
        rows[i] = { cols: new Array(50) };
        for (var j = 0; j < 50; j++) {
            rows[i].cols[j] = map[j][i];
        }
    }
    var data = {
        objMessageList: [
            {
                mapMessage: {
                    height: 50,
                    width: 50,
                    rows
                }
            }
        ]
    }
    var a = document.createElement("a");
    a.style.display = "none";
    a.href = "data:text/json;charset=utf-8," + JSON.stringify(data);
    a.download = "map.json";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

function isEmptyNearby(x, y, radius) {
    for (var i = (x - radius >= 0 ? x - radius : 0); i <= (x + radius <= 49 ? x + radius : 49); i++) {
        for (var j = (y - radius >= 0 ? y - radius : 0); j <= (y + radius <= 49 ? y + radius : 49); j++) {
            if (map[i][j] != 2) {
                return false;
            }
        }
    }
    return true;
}

function haveSthNearby(x, y, radius, type) {
    var ret = 0;
    for (var i = (x - radius >= 0 ? x - radius : 0); i <= (x + radius <= 49 ? x + radius : 49); i++) {
        for (var j = (y - radius >= 0 ? y - radius : 0); j <= (y + radius <= 49 ? y + radius : 49); j++) {
            if (map[i][j] == type) {
                ret++;
            }
        }
    }
    return ret;
}

function haveSthCross(x, y, radius, type) {
    var ret = 0;
    for (var i = (x - radius >= 0 ? x - radius : 0); i <= (x + radius <= 49 ? x + radius : 49); i++) {
        if (map[i][y] == type) {
            ret++;
        }
    }
    for (var j = (y - radius >= 0 ? y - radius : 0); j <= (y + radius <= 49 ? y + radius : 49); j++) {
        if (map[x][j] == type) {
            ret++;
        }
    }
    return ret;
}

function generateBorderBarrier() {
    for (var i = 0; i < 50; i++) {
        map[i][0] = placeType.Barrier;
        map[i][49] = placeType.Barrier;
        map[0][i] = placeType.Barrier;
        map[49][i] = placeType.Barrier;
    }
}

function generateHome() {
    map[3][46] = placeType.Home;
    map[46][3] = placeType.Home;
}


function generateEResource(num = 7) {
    for (var i = 0; i < num; i++) {
        var x = Math.floor(Math.random() * 48) + 1;
        var y = Math.floor(Math.random() * 23) + 1;
        if (isEmptyNearby(x, y, 2)) {
            map[x][y] = placeType.E_Resource;
            map[49 - x][49 - y] = placeType.E_Resource;
        }
        else {
            i--;
        }
    }
}

function generateAResource(num = 4) {
    for (var i = 0; i < num; i++) {
        var x = Math.floor(Math.random() * 48) + 1;
        var y = Math.floor(Math.random() * 23) + 1;
        if (isEmptyNearby(x, y, 2)) {
            map[x][y] = placeType.A_Resource;
            map[49 - x][49 - y] = placeType.A_Resource;
        }
        else {
            i--;
        }
    }
}
function generateConstruction(num = 5) {
    for (var i = 0; i < num; i++) {
        var x = Math.floor(Math.random() * 48) + 1;
        var y = Math.floor(Math.random() * 23) + 1;
        if (isEmptyNearby(x, y, 1)) {
            map[x][y] = placeType.Construction;
            map[49 - x][49 - y] = placeType.Construction;
        }
        else {
            i--;
        }
    }
}

function generateBush(prob = 0.015, crossBonus = 23) {
    for (var i = 0; i < 50; i++) {
        for (var j = 0; j < 50; j++) {
            if (map[i][j] == 2 && Math.random() < prob * (haveSthCross(i, j, 1, placeType.Bush) * crossBonus + 1)) {
                map[i][j] = placeType.Bush;
                map[49 - i][49 - j] = placeType.Bush;
            }
        }
    }
}

function generateBarrier(prob = 0.01, crossBonus = 40) {
    for (var i = 2; i < 48; i++) {
        for (var j = 2; j < 48; j++) {
            if ((map[i][j] == 2 || map[i][j] == 4) &&
                !haveSthNearby(i, j, 1, placeType.Home) &&
                Math.random() < prob * (haveSthCross(i, j, 1, placeType.Barrier) * (haveSthCross(i, j, 1, placeType.Barrier) > 1 ? 0 : crossBonus) + 1)) {
                map[i][j] = 3;
                map[49 - i][49 - j] = 3;
            }
        }
    }
}

function clearCanvas() {
    for (var i = 0; i < 50; i++) {
        map[i] = new Array(50);
        for (var j = 0; j < 50; j++) {
            map[i][j] = 2;
        }
    }
    // generateBorderRuin();
    // generateHome();
    draw();
}

function random() {
    for (var i = 0; i < 50; i++) {
        map[i] = new Array(50);
        for (var j = 0; j < 50; j++) {
            map[i][j] = 2;
        }
    }
    var EresourceNum = parseInt(document.getElementById("Eresource-num").value);
    var AresourceNum = parseInt(document.getElementById("Aresource-num").value);
    var constructionNum = parseInt(document.getElementById("construction-num").value);
    var bushProb = parseFloat(document.getElementById("bush-prob").value);
    var bushCrossBonus = parseInt(document.getElementById("bush-cross-bonus").value);
    var barrierProb = parseFloat(document.getElementById("barrier-prob").value);
    var barrierCrossBonus = parseInt(document.getElementById("barrier-cross-bonus").value);
    if (isNaN(EresourceNum) || EresourceNum < 1 || EresourceNum > 10) {
        EresourceNum = 7;
        alert("EResource 数量非法，设置为默认值 7");
        document.getElementById("Eresource-num").value = 7;
    }
    if (isNaN(AresourceNum) || AresourceNum < 1 || AresourceNum > 8) {
        AresourceNum = 4;
        alert("AResource 数量非法，设置为默认值 4");
        document.getElementById("Aresource-num").value = 4;
    }
    if (isNaN(constructionNum) || constructionNum < 1 || constructionNum > 10) {
        constructionNum = 5;
        alert("Construction 数量非法，设置为默认值 5");
        document.getElementById("construction-num").value = 5;
    }
    if (isNaN(bushProb) || bushProb < 0 || bushProb > 0.1) {
        bushProb = 0.015;
        alert("Bush 生成概率非法，设置为默认值 0.015");
        document.getElementById("bush-prob").value = 0.015;
    }
    if (isNaN(bushCrossBonus) || bushCrossBonus < 1 || bushCrossBonus > 50) {
        bushCrossBonus = 23;
        alert("Bush 蔓延加成非法，设置为默认值 23");
        document.getElementById("bush-cross-bonus").value = 23;
    }
    if (isNaN(barrierProb) || barrierProb < 0 || barrierProb > 0.1) {
        barrierProb = 0.01;
        alert("Barrier 生成概率非法，设置为默认值 0.01");
        document.getElementById("barrier-prob").value = 0.01;
    }
    if (isNaN(barrierCrossBonus) || barrierCrossBonus < 1 || barrierCrossBonus > 50) {
        barrierCrossBonus = 40;
        alert("Barrier 蔓延加成非法，设置为默认值 40");
        document.getElementById("barrier-cross-bonus").value = 40;
    }
    generateBorderBarrier();
    generateHome();
    generateEResource(EresourceNum);
    generateAResource(AresourceNum);
    generateConstruction(constructionNum);
    generateBush(bushProb, bushCrossBonus);
    generateBarrier(barrierProb, barrierCrossBonus);
    draw();
}

clearCanvas();

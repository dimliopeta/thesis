const mysql = require("mysql");

const db = mysql.createConnection({
    host: "localhost",
    user: "root", 
    password: "PASSWORD", 
    database: "DATABASE"
});

db.connect((err) => {
    if (err) {
        console.error("Database connection failed:", err);
        process.exit(1);
    } else {
        console.log("Connected to MySQL database!");
    }
});

module.exports = db;

// ============================================================
//  Rust Lab — 30-Minute Intro
//  Work through the numbered TODOs top-to-bottom.
//  Run the project any time with:  cargo run
// ============================================================

use rand::Rng;

fn main() {
    // ----------------------------------------------------------
    // TODO 1 — Hello, You!
    //   Replace the text below so it prints your own name.
    //   Hint: println! uses "{}" as a placeholder.
    // ----------------------------------------------------------
    let name = "Greg"; // ← change "World" to your name
    println!("Hello, {}!", name);

    // ----------------------------------------------------------
    // TODO 2 — Variables & Mutability
    //   Declare a mutable variable called `score`, set it to 0,
    //   then add 10 to it and print the result.
    //   Hint: use `let mut` to make a variable mutable.
    // ----------------------------------------------------------
    let mut score = 0;
    score += 10;
    println!("Score: {}", score);





    // ----------------------------------------------------------
    // TODO 3 — Basic Arithmetic
    //   Declare two integers, multiply them, and print the product.
    //   Example: 6 * 7 should print "The answer is 42"
    // ----------------------------------------------------------
    let int_one = 8;
    let int_two = 9;
    let product = int_one * int_two;
    println!("{} * {} = {}", int_one, int_two, product);





    // ----------------------------------------------------------
    // TODO 4 — if / else
    //   Write an if/else that prints "Even" or "Odd"
    //   for any integer you choose.
    //   Hint: use the modulo operator `%`.
    // ----------------------------------------------------------
    let task_four = rand::thread_rng().gen_range(1..=100);
    if task_four % 2 == 0 {
        println!("{} is even", task_four);
    } else {
        println!("{} is odd", task_four);
    }

    // ----------------------------------------------------------
    // TODO 5 — Loops
    //   Use a `for` loop to print the numbers 1 through 5.
    //   Hint: `for i in 1..=5 { ... }`
    // ----------------------------------------------------------
    for i in 1..=5 {
        println!("{}", i);
    }


    // ----------------------------------------------------------
    // TODO 6 — Functions
    //   Uncomment the `add` function below main, then call it
    //   here and print the result.
    //   Example: add(3, 4) should print "3 + 4 = 7"
    // ----------------------------------------------------------
    let result = add(3, 4);
    println!("3 + 4 = {}", result);

    // ----------------------------------------------------------
    // TODO 7 — Strings
    //   Create a String, append " Rust!" to it, and print it.
    //   Hint: use String::from(...) and push_str(...)
    // ----------------------------------------------------------
    let mut string7 = String::from("Hey");
    println!("{}", string7);
    string7.push_str(" Rust!");
    println!("{}", string7);



    // ----------------------------------------------------------
    // TODO 8 — Vectors
    //   Create a Vec<i32> with three numbers, push a fourth,
    //   then loop over it and print each element.
    // ----------------------------------------------------------
    let mut vec8: Vec<i32> = vec![10, 20, 30];
    vec8.push(40);
    pa(vec8);



    // ----------------------------------------------------------
    // TODO 9 — Structs
    //   Uncomment the `Point` struct below main, create an
    //   instance of it, and print its x and y fields.
    // ----------------------------------------------------------
    let p9 = Point { x: 1, y: 2 };
    println!("Point x is {}, y is {}", p9.x, p9.y);

    // ----------------------------------------------------------
    // TODO 10 — Enums & Match
    //   Uncomment the `Direction` enum below main.
    //   Create a variable of that type and use a `match`
    //   expression to print which direction it is.
    // ----------------------------------------------------------
    let d10 = Direction::Up;
    match d10 {
        Direction::Up => println!("Going up!"),
        Direction::Down => println!("Going down!"),
        Direction::Left => println!("Going left!"),
        Direction::Right => println!("Going right!"),
    }

    // ----------------------------------------------------------
    // TODO 11 - Traits (define shared behavior)
    //   1) Add a second type called Cat with one field: name: String
    //   2) Implement the Speaker trait for Cat
    //   3) Change Cat's speak output to "<name> says meow"
    //   4) Create a Cat instance here and print its speech
    // ----------------------------------------------------------
    let dog = Dog {
        name: String::from("Rex"),
    };
    println!("{}", dog.speak());

    // ----------------------------------------------------------
    // TODO 12 - Impl Blocks (associated fn + methods)
    //   1) Add a method: fn can_hold(&self, other: &Rectangle) -> bool
    //      Return true when self can fully contain other.
    //   2) Add an associated fn: fn square(size: f64) -> Self
    //   3) Create a square below and print whether it can hold r1
    // ----------------------------------------------------------
    let r1 = Rectangle::new(8.0, 3.0);
    println!("r1 area: {}", r1.area());

    // TODO 13 - Traits + Generics
    //   Write a generic function below main:
    //   fn announce<T: Speaker>(item: &T) { ... }
    //   Then call announce(&dog) here.

    // TODO 14 - Default Trait Methods
    //   Add fn category(&self) -> &'static str to Speaker with a default value,
    //   then override it for Dog and (your new) Cat.
    //   Print both categories here.
}

// ── Helpers for the TODOs above ─────────────────────────────

// TODO 6 — define your add function here
fn add(a: i32, b: i32) -> i32 {
    a + b
}

// TODO 9 — define your Point struct here
struct Point {
    x: i32,
    y: i32,
}

// TODO 10 — define your Direction enum here
enum Direction {
    Up,
    Down,
    Left,
    Right,
}

trait Speaker {
    fn speak(&self) -> String;
}

struct Dog {
    name: String,
}

impl Speaker for Dog {
    fn speak(&self) -> String {
        format!("{} says woof", self.name)
    }
}

struct Rectangle {
    width: f64,
    height: f64,
}

impl Rectangle {
    fn new(width: f64, height: f64) -> Self {
        Self { width, height }
    }

    fn area(&self) -> f64 {
        self.width * self.height
    }
}

fn pv(x: &str) {
    println!("{}", x);
}

fn pa(a: Vec<i32>) {
    for n in &a {
        println!("{}", n);
    }
}


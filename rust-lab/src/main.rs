// ============================================================
//  Rust Lab — 30-Minute Intro
//  Work through the numbered TODOs top-to-bottom.
//  Run the project any time with:  cargo run
// ============================================================

fn main() {
    // ----------------------------------------------------------
    // TODO 1 — Hello, You!
    //   Replace the text below so it prints your own name.
    //   Hint: println! uses "{}" as a placeholder.
    // ----------------------------------------------------------
    let name = "World"; // ← change "World" to your name
    println!("Hello, {}!", name);

    // ----------------------------------------------------------
    // TODO 2 — Variables & Mutability
    //   Declare a mutable variable called `score`, set it to 0,
    //   then add 10 to it and print the result.
    //   Hint: use `let mut` to make a variable mutable.
    // ----------------------------------------------------------
    // let mut score = 0;
    // score += 10;
    // println!("Score: {}", score);

    // ----------------------------------------------------------
    // TODO 3 — Basic Arithmetic
    //   Declare two integers, multiply them, and print the product.
    //   Example: 6 * 7 should print "The answer is 42"
    // ----------------------------------------------------------
    // let a = 6;
    // let b = 7;
    // println!("The answer is {}", a * b);

    // ----------------------------------------------------------
    // TODO 4 — if / else
    //   Write an if/else that prints "Even" or "Odd"
    //   for any integer you choose.
    //   Hint: use the modulo operator `%`.
    // ----------------------------------------------------------
    // let number = 7;
    // if number % 2 == 0 {
    //     println!("Even");
    // } else {
    //     println!("Odd");
    // }

    // ----------------------------------------------------------
    // TODO 5 — Loops
    //   Use a `for` loop to print the numbers 1 through 5.
    //   Hint: `for i in 1..=5 { ... }`
    // ----------------------------------------------------------
    // for i in 1..=5 {
    //     println!("{}", i);
    // }

    // ----------------------------------------------------------
    // TODO 6 — Functions
    //   Uncomment the `add` function below main, then call it
    //   here and print the result.
    //   Example: add(3, 4) should print "3 + 4 = 7"
    // ----------------------------------------------------------
    // let result = add(3, 4);
    // println!("3 + 4 = {}", result);

    // ----------------------------------------------------------
    // TODO 7 — Strings
    //   Create a String, append " Rust!" to it, and print it.
    //   Hint: use String::from(...) and push_str(...)
    // ----------------------------------------------------------
    // let mut greeting = String::from("Hello,");
    // greeting.push_str(" Rust!");
    // println!("{}", greeting);

    // ----------------------------------------------------------
    // TODO 8 — Vectors
    //   Create a Vec<i32> with three numbers, push a fourth,
    //   then loop over it and print each element.
    // ----------------------------------------------------------
    // let mut numbers: Vec<i32> = vec![10, 20, 30];
    // numbers.push(40);
    // for n in &numbers {
    //     println!("{}", n);
    // }

    // ----------------------------------------------------------
    // TODO 9 — Structs
    //   Uncomment the `Point` struct below main, create an
    //   instance of it, and print its x and y fields.
    // ----------------------------------------------------------
    // let p = Point { x: 3, y: 7 };
    // println!("x={}, y={}", p.x, p.y);

    // ----------------------------------------------------------
    // TODO 10 — Enums & Match
    //   Uncomment the `Direction` enum below main.
    //   Create a variable of that type and use a `match`
    //   expression to print which direction it is.
    // ----------------------------------------------------------
    // let dir = Direction::North;
    // match dir {
    //     Direction::North => println!("Heading North"),
    //     Direction::South => println!("Heading South"),
    //     Direction::East  => println!("Heading East"),
    //     Direction::West  => println!("Heading West"),
    // }
}

// ── Helpers for the TODOs above ─────────────────────────────

// TODO 6 — uncomment this function
// fn add(a: i32, b: i32) -> i32 {
//     a + b
// }

// TODO 9 — uncomment this struct
// struct Point {
//     x: i32,
//     y: i32,
// }

// TODO 10 — uncomment this enum
// enum Direction {
//     North,
//     South,
//     East,
//     West,
// }


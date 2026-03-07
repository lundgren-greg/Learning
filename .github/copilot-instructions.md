# GitHub Copilot Instructions

## Repository Overview

This repository is a collection of learning projects and experiments. The primary project is **rust-lab**, a 30-minute hands-on introduction to Rust fundamentals.

## Rust Lab Context

The `rust-lab` project (`rust-lab/src/main.rs`) contains 10 numbered TODO tasks that guide a learner through core Rust concepts:

| Task | Concept |
|------|---------|
| TODO 1 | Variables and `println!` |
| TODO 2 | Mutability (`let mut`) |
| TODO 3 | Basic arithmetic |
| TODO 4 | `if` / `else` and the `%` operator |
| TODO 5 | `for` loops and ranges (`1..=5`) |
| TODO 6 | Functions with parameters and return types |
| TODO 7 | `String::from` and `push_str` |
| TODO 8 | `Vec<T>`, `push`, and iteration with `&` |
| TODO 9 | Structs and field access |
| TODO 10 | Enums and `match` expressions |

## How to Use GitHub Copilot on This Project

### Getting Unstuck on a TODO

Place your cursor inside a TODO block and ask Copilot in chat:

```
Explain what TODO 5 is asking me to do and show me a minimal example.
```

### Generating a First Draft

Highlight the commented-out code for a TODO and use the inline chat shortcut
(`Ctrl+I` / `Cmd+I`) with a prompt like:

```
Uncomment and complete TODO 8: create a Vec<i32>, push a value, and print each element.
```

### Explaining Rust Syntax

Select any snippet and ask:

```
Explain this Rust code line by line.
```

### Running the Project

After completing each task, verify it compiles and runs:

```bash
cd rust-lab
cargo run
```

## Copilot Coding Guidelines for This Repo

- **Minimal changes only** — complete each TODO without modifying other tasks.
- **No external crates** — keep `Cargo.toml` dependency-free; use the Rust standard library only.
- **Idiomatic Rust** — prefer `let` bindings, owned types, and standard iterators as shown in the hints.
- **Comments** — preserve the existing TODO comment headers so the file remains readable as a tutorial.

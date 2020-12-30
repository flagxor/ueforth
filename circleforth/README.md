# Circleforth

Created as an example of a minimalist Forth in the style of typical "toy" Lisp implementations.

Similar to classic "Eval-Apply" style Lisps, the core problem of reimplementing the core interpreter
is approached on top of an existing Forth. Primitives are reused, and crucially parsing is reused.

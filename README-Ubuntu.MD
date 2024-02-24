# Building on Ubuntu 22.04 LTS "Jammy Jellyfish"

1. Clone this repository ***with submodule recursion!***

    ```
    git clone https://github.com/flagxor/ueforth --recursive
    ```

    This takes a long time, but you need all that to build the `pico-ice` binary. When it's done, `cd ueforth`.

2. Run `./jammy-dependencies.sh`. This will install everything you need to build.
3. Run `./configure.py`. This will check for errors and set up the BUILD descriptors.
4. Run `ninja 2>&1 | tee ninja-jammy.log`. This will build everything except the Windows version and capture a logfile.
5. Run `ls -lR out | tee jammy-outputs.log`. This will list the outputs and capture a logfile.

At this point, the `posix`, `esp32-sim` and `pico-ice-sim` builds should run on your machine.

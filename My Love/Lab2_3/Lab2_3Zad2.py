import math
import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_3, вариант 8 — Задание 2")
    root.geometry("720x380")

    ttk.Label(root, text="Задание 2.\nВвести 5 целых чисел.\nПоказать квадрат и корень (если можно).").pack(
        anchor="w", padx=10, pady=(10, 6)
    )

    ttk.Label(root, text="Числа (через пробел):").pack(anchor="w", padx=10)
    ent = ttk.Entry(root)
    ent.pack(fill="x", padx=10, pady=(4, 8))
    ent.insert(0, "1 2 3 4 5")
    ent.focus_set()

    mode = tk.StringVar(value="both")  # radiobutton
    opts = ttk.LabelFrame(root, text="Что вывести")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Квадрат и корень", variable=mode, value="both").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только квадрат", variable=mode, value="sq").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только корень", variable=mode, value="rt").pack(
        side="left", padx=8, pady=6
    )

    out = tk.Text(root, height=10, wrap="none")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def run():
        s = ent.get().strip()
        parts = s.split()
        if len(parts) != 5:
            messagebox.showerror("Ошибка", "Нужно ввести ровно 5 чисел.")
            return
        try:
            xs = [int(p) for p in parts]
        except Exception:
            messagebox.showerror("Ошибка", "Не получилось прочитать числа.")
            return

        out.delete("1.0", "end")
        for x in xs:
            sq = x * x
            if x >= 0:
                rt = math.sqrt(x)
                rt_s = f"{rt:.5f}"
            else:
                rt_s = "нет (x<0)"

            if mode.get() == "sq":
                out.insert("end", f"x={x}  x^2={sq}\n")
            elif mode.get() == "rt":
                out.insert("end", f"x={x}  sqrt={rt_s}\n")
            else:
                out.insert("end", f"x={x}  x^2={sq}  sqrt={rt_s}\n")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()


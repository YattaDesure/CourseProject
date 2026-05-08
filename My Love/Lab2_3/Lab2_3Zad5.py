import random
import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_3, вариант 8 — Задание 5")
    root.geometry("820x420")

    ttk.Label(
        root,
        text="Задание 5.\nМассив (20 вещественных), случайные 50..100.\nНайти сумму элементов > заданного числа.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="Порог:").pack(side="left")
    ent_t = ttk.Entry(row, width=10)
    ent_t.pack(side="left", padx=8)
    ent_t.insert(0, "70")

    mode = tk.StringVar(value="rnd")  # radiobutton
    opts = ttk.LabelFrame(root, text="Массив")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Случайный", variable=mode, value="rnd").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Ввести свой", variable=mode, value="my").pack(
        side="left", padx=8, pady=6
    )

    ttk.Label(root, text="Если свой: 20 чисел через пробел:").pack(anchor="w", padx=10)
    ent_arr = ttk.Entry(root)
    ent_arr.pack(fill="x", padx=10, pady=(4, 8))

    out = tk.Text(root, height=12, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def run():
        try:
            t = float(ent_t.get().strip())
        except Exception:
            messagebox.showerror("Ошибка", "Порог должен быть числом.")
            return

        if mode.get() == "my":
            parts = ent_arr.get().strip().split()
            if len(parts) != 20:
                messagebox.showerror("Ошибка", "Нужно ввести ровно 20 чисел.")
                return
            try:
                a = [float(p) for p in parts]
            except Exception:
                messagebox.showerror("Ошибка", "Не получилось прочитать массив.")
                return
        else:
            a = []
            for _ in range(20):
                a.append(random.uniform(50, 100))

        s = 0.0
        for x in a:
            if x > t:
                s += x

        out.delete("1.0", "end")
        out.insert("end", "Массив:\n")
        out.insert("end", " ".join(f"{x:.2f}" for x in a) + "\n\n")
        out.insert("end", f"Порог = {t}\nСумма элементов > порога = {s:.2f}\n")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()


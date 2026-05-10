import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_4, вариант 8 — Задание 2")
    root.geometry("680x420")

    ttk.Label(
        root,
        text="Задание 2.\nДружественные числа: a и b — каждое равно сумме делителей другого (без самого числа).\nНайти такие пары в диапазоне 200..300.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="От:").pack(side="left")
    ent_a = ttk.Entry(row, width=8)
    ent_a.pack(side="left", padx=4)
    ent_a.insert(0, "200")
    ttk.Label(row, text="До:").pack(side="left", padx=(10, 0))
    ent_b = ttk.Entry(row, width=8)
    ent_b.pack(side="left", padx=4)
    ent_b.insert(0, "300")

    mode = tk.StringVar(value="strict")  # radiobutton
    opts = ttk.LabelFrame(root, text="Что выводить")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Только пары в диапазоне", variable=mode, value="strict").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Если хотя бы одно в диапазоне", variable=mode, value="any").pack(
        side="left", padx=8, pady=6
    )

    out = tk.Text(root, height=12, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def sum_div(n):
        s = 0
        for d in range(1, n):
            if n % d == 0:
                s += d
        return s

    def run():
        try:
            a = int(ent_a.get().strip())
            b = int(ent_b.get().strip())
        except Exception:
            messagebox.showerror("Ошибка", "От и До должны быть числами.")
            return
        if a > b:
            a, b = b, a

        out.delete("1.0", "end")
        found = 0
        for x in range(a, b + 1):
            y = sum_div(x)
            if y <= x:
                continue  # чтобы не дублировать пары
            if sum_div(y) != x:
                continue
            if mode.get() == "strict":
                if y < a or y > b:
                    continue
            out.insert("end", f"{x} и {y}\n")
            found += 1

        if found == 0:
            out.insert("end", "Пар не найдено.\n")
        else:
            out.insert("end", f"\nВсего пар: {found}\n")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()

import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_3, вариант 8 — Задание 3")
    root.geometry("860x520")

    ttk.Label(
        root,
        text="Задание 3.\nПроверка билета.\nДано N чисел (0/1) и K.\nЧисла на расстоянии K должны совпадать.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="K:").pack(side="left")
    ent_k = ttk.Entry(row, width=10)
    ent_k.pack(side="left", padx=8)
    ent_k.insert(0, "2")

    ttk.Label(root, text="Последовательность 0/1 (через пробел):").pack(anchor="w", padx=10)
    ent_seq = ttk.Entry(root)
    ent_seq.pack(fill="x", padx=10, pady=(4, 8))
    ent_seq.insert(0, "0 1 0 1 0 1")
    ent_seq.focus_set()

    mode = tk.StringVar(value="one")  # radiobutton
    opts = ttk.LabelFrame(root, text="Ошибки")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Разрешить 1 ошибку", variable=mode, value="one").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Без ошибок", variable=mode, value="zero").pack(
        side="left", padx=8, pady=6
    )

    out = tk.Text(root, height=12, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def ok_seq(a, k):
        n = len(a)
        for i in range(n - k):
            if a[i] != a[i + k]:
                return False
        return True

    def run():
        try:
            k = int(ent_k.get().strip())
        except Exception:
            messagebox.showerror("Ошибка", "K должно быть числом.")
            return
        if k <= 0:
            messagebox.showerror("Ошибка", "K должно быть > 0.")
            return

        parts = ent_seq.get().strip().split()
        if not parts:
            messagebox.showerror("Ошибка", "Введите последовательность.")
            return
        try:
            a = [int(p) for p in parts]
        except Exception:
            messagebox.showerror("Ошибка", "Нужно вводить 0/1.")
            return
        for x in a:
            if x not in (0, 1):
                messagebox.showerror("Ошибка", "Нужно вводить только 0 или 1.")
                return

        out.delete("1.0", "end")

        if k >= len(a):
            out.insert("end", "K >= N, проверять нечего -> считаем подлинным.\n")
            return

        if ok_seq(a, k):
            out.insert("end", "Подлинный. Ошибок не видно.\n")
            return

        if mode.get() == "zero":
            out.insert("end", "Не подлинный (есть несовпадения).\n")
            return

        # пробуем исправить 1 ошибку
        found = []
        for i in range(len(a)):
            b = a[:]
            b[i] = 1 - b[i]
            if ok_seq(b, k):
                found.append(i + 1)  # позиции от 1

        if not found:
            out.insert("end", "Не подлинный. Даже с 1 ошибкой не сходится.\n")
        else:
            out.insert("end", "Подлинный, если была 1 ошибка.\n")
            out.insert("end", "Ошибка могла быть в позиции(ях): " + ", ".join(map(str, found)) + "\n")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()


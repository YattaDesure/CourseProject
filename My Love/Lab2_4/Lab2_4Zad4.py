import tkinter as tk
from tkinter import filedialog, messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_4, вариант 8 — Задание 4")
    root.geometry("760x460")

    ttk.Label(
        root,
        text="Задание 4.\nИз input.txt прочитать массив (15 вещественных чисел).\n"
        "Найти разность: (произведение положительных) − (произведение модулей отрицательных).",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    file_path = tk.StringVar(value="input.txt")
    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="Файл:").pack(side="left")
    ttk.Entry(row, textvariable=file_path).pack(side="left", fill="x", expand=True, padx=8)
    ttk.Button(row, text="Выбрать...", command=lambda: pick()).pack(side="left")

    mode = tk.StringVar(value="diff")  # radiobutton
    opts = ttk.LabelFrame(root, text="Что показать")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Только разность", variable=mode, value="diff").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Все детали", variable=mode, value="full").pack(
        side="left", padx=8, pady=6
    )

    out = tk.Text(root, height=12, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def pick():
        p = filedialog.askopenfilename(title="Выберите input.txt")
        if p:
            file_path.set(p)

    def run():
        try:
            with open(file_path.get().strip(), "r", encoding="utf-8") as f:
                text = f.read()
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))
            return

        parts = text.replace(",", " ").split()
        if len(parts) != 15:
            messagebox.showerror(
                "Ошибка",
                f"В файле должно быть 15 чисел (нашлось {len(parts)}).",
            )
            return
        try:
            a = [float(p) for p in parts]
        except Exception:
            messagebox.showerror("Ошибка", "Не получилось прочитать как числа.")
            return

        prod_pos = 1.0
        has_pos = False
        prod_neg = 1.0
        has_neg = False
        for x in a:
            if x > 0:
                prod_pos *= x
                has_pos = True
            elif x < 0:
                prod_neg *= -x
                has_neg = True

        if not has_pos:
            prod_pos = 0.0
        if not has_neg:
            prod_neg = 0.0

        diff = prod_pos - prod_neg

        out.delete("1.0", "end")
        if mode.get() == "full":
            out.insert("end", "Массив: " + " ".join(f"{x:g}" for x in a) + "\n")
            out.insert("end", f"Произведение положительных: {prod_pos:g}\n")
            out.insert("end", f"Произведение модулей отрицательных: {prod_neg:g}\n")
        out.insert("end", f"Разность = {diff:g}\n")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.mainloop()


if __name__ == "__main__":
    main()

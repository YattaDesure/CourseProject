import tkinter as tk
from tkinter import filedialog, messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_2, вариант 8 — Задание 5")
    root.geometry("740x420")

    file_path = tk.StringVar(value="")
    what = tk.StringVar(value="both")  # radiobutton

    ttk.Label(
        root,
        text="Задание 5.\nФайл с целыми числами (по одному в строке).\nНайти сумму и количество.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="Файл:").pack(side="left")
    ttk.Entry(row, textvariable=file_path).pack(side="left", fill="x", expand=True, padx=8)
    ttk.Button(row, text="Выбрать...", command=lambda: pick()).pack(side="left")

    opts = ttk.LabelFrame(root, text="Что вывести")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Сумма и количество", variable=what, value="both").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только сумма", variable=what, value="sum").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только количество", variable=what, value="count").pack(
        side="left", padx=8, pady=6
    )

    res = ttk.Label(root, text="Результат: ")
    res.pack(anchor="w", padx=10, pady=6)

    out = tk.Text(root, height=10, wrap="none")
    out.pack(fill="both", expand=True, padx=10, pady=(0, 10))

    def pick():
        p = filedialog.askopenfilename(title="Выберите текстовый файл")
        if p:
            file_path.set(p)
            show_file()

    def show_file():
        out.delete("1.0", "end")
        p = file_path.get().strip()
        if not p:
            return
        try:
            with open(p, "r", encoding="utf-8") as f:
                out.insert("1.0", f.read())
        except Exception as e:
            out.insert("1.0", f"Не удалось прочитать файл: {e}")

    def run():
        p = file_path.get().strip()
        if not p:
            messagebox.showerror("Ошибка", "Сначала выбери файл.")
            return
        try:
            with open(p, "r", encoding="utf-8") as f:
                lines = f.readlines()
            total = 0
            cnt = 0
            for line in lines:
                s = line.strip()
                if not s:
                    continue
                total += int(s)
                cnt += 1
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))
            return

        if what.get() == "sum":
            res.configure(text=f"Результат: сумма = {total}")
        elif what.get() == "count":
            res.configure(text=f"Результат: количество = {cnt}")
        else:
            res.configure(text=f"Результат: сумма = {total}, количество = {cnt}")

    btns = ttk.Frame(root)
    btns.pack(fill="x", padx=10, pady=6)
    ttk.Button(btns, text="Ок", command=run).pack(side="left")
    ttk.Button(btns, text="Показать файл", command=show_file).pack(side="left", padx=8)

    root.mainloop()


if __name__ == "__main__":
    main()


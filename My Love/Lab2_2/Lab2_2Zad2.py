import tkinter as tk
from tkinter import filedialog, messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_2, вариант 8 — Задание 2")
    root.geometry("700x360")

    file_path = tk.StringVar(value="")
    where = tk.StringVar(value="end")  # radiobutton

    ttk.Label(
        root,
        text="Задание 2.\nДобавить строку в начало или в конец файла.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="Файл:").pack(side="left")
    ttk.Entry(row, textvariable=file_path).pack(side="left", fill="x", expand=True, padx=8)
    ttk.Button(row, text="Выбрать...", command=lambda: pick()).pack(side="left")

    row2 = ttk.Frame(root)
    row2.pack(fill="x", padx=10, pady=6)
    ttk.Label(row2, text="Строка S:").pack(side="left")
    ent_s = ttk.Entry(row2)
    ent_s.pack(side="left", fill="x", expand=True, padx=8)
    ent_s.focus_set()

    opts = ttk.LabelFrame(root, text="Куда добавить")
    opts.pack(fill="x", padx=10, pady=8)
    ttk.Radiobutton(opts, text="В начало", variable=where, value="start").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="В конец", variable=where, value="end").pack(
        side="left", padx=8, pady=6
    )

    out = tk.Text(root, height=8, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

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
        s = ent_s.get().rstrip("\n") + "\n"

        try:
            try:
                with open(p, "r", encoding="utf-8") as f:
                    old = f.read()
            except FileNotFoundError:
                old = ""

            if where.get() == "start":
                new = s + old
            else:
                new = old + s

            with open(p, "w", encoding="utf-8") as f:
                f.write(new)
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))
            return

        messagebox.showinfo("Готово", "Строка добавлена.")
        show_file()

    btns = ttk.Frame(root)
    btns.pack(fill="x", padx=10, pady=6)
    ttk.Button(btns, text="Ок", command=run).pack(side="left")
    ttk.Button(btns, text="Показать файл", command=show_file).pack(side="left", padx=8)

    root.mainloop()


if __name__ == "__main__":
    main()


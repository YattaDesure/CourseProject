import tkinter as tk
from tkinter import filedialog, messagebox, ttk


FIELDS = [
    ("Фамилия", "surname"),
    ("Имя", "name"),
    ("Отчество", "patronymic"),
    ("Пол", "sex"),
    ("Национальность", "nat"),
    ("Рост", "height"),
    ("Вес", "weight"),
    ("Дата рождения (ГГГГ-ММ-ДД)", "birth"),
    ("Телефон", "phone"),
    ("Команда", "team"),
    ("Номер в команде", "num"),
    ("Амплуа", "role"),
    ("Очки", "points"),
    ("Игр", "games"),
]


def main():
    root = tk.Tk()
    root.title("ЛР 2_4, вариант 8 — Задание 3 (Баскетболисты)")
    root.geometry("900x680")

    ttk.Label(
        root,
        text="Задание 3.\nДобавлять баскетболистов в файл, читать из файла,\n"
        "и сохранять в новый файл тех, кто забросил больше 150 очков за свою команду.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    # путь к исходному файлу
    file_path = tk.StringVar(value="basket.txt")
    out_path = tk.StringVar(value="basket_top.txt")

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=4)
    ttk.Label(row, text="Файл данных:").pack(side="left")
    ttk.Entry(row, textvariable=file_path).pack(side="left", fill="x", expand=True, padx=8)
    ttk.Button(row, text="Выбрать...", command=lambda: pick(file_path)).pack(side="left")

    row2 = ttk.Frame(root)
    row2.pack(fill="x", padx=10, pady=4)
    ttk.Label(row2, text="Файл для топа:").pack(side="left")
    ttk.Entry(row2, textvariable=out_path).pack(side="left", fill="x", expand=True, padx=8)

    # форма добавления
    form = ttk.LabelFrame(root, text="Добавить баскетболиста")
    form.pack(fill="x", padx=10, pady=6)

    entries = {}
    for i, (label, key) in enumerate(FIELDS):
        r, c = divmod(i, 2)
        ttk.Label(form, text=label + ":").grid(row=r, column=c * 2, sticky="w", padx=6, pady=2)
        e = ttk.Entry(form, width=24)
        e.grid(row=r, column=c * 2 + 1, padx=6, pady=2, sticky="we")
        entries[key] = e
    for c in (1, 3):
        form.columnconfigure(c, weight=1)

    # режим вывода
    mode = tk.StringVar(value="top")  # radiobutton
    opts = ttk.LabelFrame(root, text="Показать")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Все из файла", variable=mode, value="all").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только > 150 очков", variable=mode, value="top").pack(
        side="left", padx=8, pady=6
    )

    out = tk.Text(root, height=12, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def pick(var):
        p = filedialog.askopenfilename()
        if p:
            var.set(p)

    def add_one():
        # склеиваем в одну строку через ;
        parts = []
        for _, key in FIELDS:
            parts.append(entries[key].get().strip())
        line = ";".join(parts)

        try:
            with open(file_path.get().strip(), "a", encoding="utf-8") as f:
                f.write(line + "\n")
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))
            return

        for _, key in FIELDS:
            entries[key].delete(0, "end")
        messagebox.showinfo("Готово", "Добавлено в файл.")

    def parse_line(line):
        parts = line.rstrip("\n").split(";")
        d = {}
        for i, (_, key) in enumerate(FIELDS):
            d[key] = parts[i] if i < len(parts) else ""
        return d

    def show_file():
        try:
            with open(file_path.get().strip(), "r", encoding="utf-8") as f:
                lines = f.readlines()
        except FileNotFoundError:
            messagebox.showerror("Ошибка", "Файл не найден.")
            return
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))
            return

        out.delete("1.0", "end")
        cnt = 0
        top = []
        for line in lines:
            if not line.strip():
                continue
            d = parse_line(line)
            try:
                pts = int(d.get("points", "0") or 0)
            except Exception:
                pts = 0

            if mode.get() == "top" and pts <= 150:
                continue

            cnt += 1
            out.insert(
                "end",
                f"{cnt}) {d.get('surname','')} {d.get('name','')} — команда {d.get('team','')}, очки: {pts}\n",
            )
            if pts > 150:
                top.append(line.rstrip("\n"))

        if mode.get() == "top":
            try:
                with open(out_path.get().strip(), "w", encoding="utf-8") as f:
                    for line in top:
                        f.write(line + "\n")
                out.insert("end", f"\nСохранено в {out_path.get()}: {len(top)}\n")
            except Exception as e:
                messagebox.showerror("Ошибка", str(e))

    btns = ttk.Frame(root)
    btns.pack(fill="x", padx=10, pady=(0, 6))
    ttk.Button(btns, text="Добавить", command=add_one).pack(side="left")
    ttk.Button(btns, text="Ок (показать)", command=show_file).pack(side="left", padx=8)

    root.mainloop()


if __name__ == "__main__":
    main()

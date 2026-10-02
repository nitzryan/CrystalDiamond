type SectionSelectorConfig = {
    groupId : string
    initial : string
    sections : Record<string, HTMLElement[]>
}

class SectionSelector
{
    selected : string

    private groupId : string
    private sections : Record<string, HTMLElement[]>

    constructor(config : SectionSelectorConfig)
    {
        this.groupId = config.groupId
        this.sections = config.sections

        const keys = Object.keys(config.sections)
        this.selected = keys.includes(config.initial) ? config.initial : keys[0]

        document.querySelectorAll<HTMLButtonElement>(`#${this.groupId} button`)
            .forEach(btn => {
                btn.addEventListener('click', () => {
                    this.select(btn.getAttribute('data-section') ?? '')
                })
            })

        this.sync()
    }

    select(section : string)
    {
        if (!Object.keys(this.sections).includes(section))
            return
        this.selected = section
        this.sync()
    }

    private sync()
    {
        document.querySelectorAll<HTMLButtonElement>(`#${this.groupId} button`)
            .forEach(btn => btn.classList.toggle('selected',
                btn.getAttribute('data-section') === this.selected))

        const visible = new Set(this.sections[this.selected])
        for (const elements of Object.values(this.sections))
            for (const el of elements)
                el.classList.toggle('hidden', !visible.has(el))
    }


}
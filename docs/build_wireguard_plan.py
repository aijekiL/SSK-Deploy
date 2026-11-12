from docx import Document
from docx.shared import Cm, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.enum.section import WD_SECTION
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.enum.style import WD_STYLE_TYPE
from docx.enum.section import WD_ORIENT
from docx.enum.text import WD_LINE_SPACING
from pathlib import Path

OUT = Path(r"E:\Iot\SSK-Deploy20260304\output\WireGuard远程接入实施方案.docx")

NAVY = "17365D"
BLUE = "DCE6F1"
PALE = "F4F7FA"
GRID = "D9D9D9"
TEXT = "1F2937"
WHITE = "FFFFFF"

def set_cell_shading(cell, fill):
    tc_pr = cell._tc.get_or_add_tcPr()
    shd = OxmlElement('w:shd')
    shd.set(qn('w:fill'), fill)
    tc_pr.append(shd)

def set_cell_margin(cell, top=100, start=120, bottom=100, end=120):
    tc = cell._tc
    tc_pr = tc.get_or_add_tcPr()
    tc_mar = tc_pr.first_child_found_in('w:tcMar')
    if tc_mar is None:
        tc_mar = OxmlElement('w:tcMar')
        tc_pr.append(tc_mar)
    for side, val in (("top", top), ("start", start), ("bottom", bottom), ("end", end)):
        node = tc_mar.find(qn(f'w:{side}'))
        if node is None:
            node = OxmlElement(f'w:{side}')
            tc_mar.append(node)
        node.set(qn('w:w'), str(val))
        node.set(qn('w:type'), 'dxa')

def set_table_borders(table, color=GRID, size='6'):
    tbl_pr = table._tbl.tblPr
    borders = tbl_pr.first_child_found_in('w:tblBorders')
    if borders is None:
        borders = OxmlElement('w:tblBorders')
        tbl_pr.append(borders)
    for edge in ('top', 'left', 'bottom', 'right', 'insideH', 'insideV'):
        tag = qn(f'w:{edge}')
        node = borders.find(tag)
        if node is None:
            node = OxmlElement(f'w:{edge}')
            borders.append(node)
        node.set(qn('w:val'), 'single')
        node.set(qn('w:sz'), size)
        node.set(qn('w:space'), '0')
        node.set(qn('w:color'), color)

def set_repeat_table_header(row):
    tr_pr = row._tr.get_or_add_trPr()
    tbl_header = OxmlElement('w:tblHeader')
    tbl_header.set(qn('w:val'), 'true')
    tr_pr.append(tbl_header)

def set_row_cant_split(row):
    tr_pr = row._tr.get_or_add_trPr()
    cant_split = OxmlElement('w:cantSplit')
    tr_pr.append(cant_split)

def set_keep_with_next(paragraph):
    ppr = paragraph._p.get_or_add_pPr()
    keep = OxmlElement('w:keepNext')
    ppr.append(keep)

def set_cjk_font(run, font='Microsoft YaHei'):
    run.font.name = font
    rpr = run._element.get_or_add_rPr()
    rfonts = rpr.rFonts
    if rfonts is None:
        rfonts = OxmlElement('w:rFonts')
        rpr.append(rfonts)
    rfonts.set(qn('w:eastAsia'), font)
    rfonts.set(qn('w:ascii'), font)
    rfonts.set(qn('w:hAnsi'), font)

def style_run(run, size=10.5, bold=False, color=TEXT, font='Microsoft YaHei'):
    set_cjk_font(run, font)
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.color.rgb = RGBColor.from_string(color)

def add_text(p, text, size=10.5, bold=False, color=TEXT, font='Microsoft YaHei'):
    r = p.add_run(text)
    style_run(r, size, bold, color, font)
    return r

def add_para(doc, text='', before=0, after=6, indent=0, first=0, align=None, size=10.5, line=1.35):
    p = doc.add_paragraph()
    pf = p.paragraph_format
    pf.space_before = Pt(before)
    pf.space_after = Pt(after)
    pf.left_indent = Cm(indent)
    pf.first_line_indent = Cm(first)
    pf.line_spacing = line
    if align is not None:
        p.alignment = align
    if text:
        add_text(p, text, size=size)
    return p

def add_bullet(doc, text, level=0):
    p = doc.add_paragraph(style='List Bullet' if level == 0 else 'List Bullet 2')
    p.paragraph_format.space_after = Pt(3)
    p.paragraph_format.line_spacing = 1.25
    add_text(p, text)
    return p

def add_number(doc, text):
    p = doc.add_paragraph(style='List Number')
    p.paragraph_format.space_after = Pt(3)
    p.paragraph_format.line_spacing = 1.25
    add_text(p, text)
    return p

def add_code(doc, code):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(3)
    p.paragraph_format.space_after = Pt(6)
    p.paragraph_format.left_indent = Cm(0.3)
    p.paragraph_format.right_indent = Cm(0.3)
    p.paragraph_format.line_spacing = 1.08
    ppr = p._p.get_or_add_pPr()
    shd = OxmlElement('w:shd')
    shd.set(qn('w:fill'), 'F2F4F7')
    ppr.append(shd)
    lines = code.strip('\n').split('\n')
    for index, line in enumerate(lines):
        r = p.add_run(line)
        style_run(r, size=8.5, color='243447', font='Consolas')
        if index < len(lines) - 1:
            r.add_break()
    return p

def add_table(doc, headers, rows, widths=None, font_size=9):
    table = doc.add_table(rows=1, cols=len(headers))
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    set_table_borders(table)
    hdr = table.rows[0]
    set_repeat_table_header(hdr)
    for i, head in enumerate(headers):
        cell = hdr.cells[i]
        if widths:
            cell.width = Cm(widths[i])
        cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
        set_cell_shading(cell, NAVY)
        set_cell_margin(cell)
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_after = Pt(0)
        add_text(p, head, size=font_size, bold=True, color=WHITE)
    for row_no, row in enumerate(rows):
        added_row = table.add_row()
        set_row_cant_split(added_row)
        cells = added_row.cells
        for i, value in enumerate(row):
            cell = cells[i]
            if widths:
                cell.width = Cm(widths[i])
            cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
            set_cell_margin(cell)
            if row_no % 2 == 1:
                set_cell_shading(cell, PALE)
            p = cell.paragraphs[0]
            p.paragraph_format.space_after = Pt(0)
            p.paragraph_format.line_spacing = 1.12
            # List values are rendered as compact multi-line content.
            lines = value if isinstance(value, list) else str(value).split('\n')
            for li, line in enumerate(lines):
                if li:
                    p.add_run().add_break()
                add_text(p, str(line), size=font_size)
    doc.add_paragraph().paragraph_format.space_after = Pt(2)
    return table

def heading(doc, text, level=1):
    p = doc.add_paragraph(style=f'Heading {level}')
    p.paragraph_format.space_before = Pt(14 if level == 1 else 9)
    p.paragraph_format.space_after = Pt(6 if level == 1 else 4)
    set_keep_with_next(p)
    r = p.add_run(text)
    style_run(r, size=15 if level == 1 else 12, bold=True, color='000000')
    return p

def add_page_number(paragraph):
    paragraph.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    r = paragraph.add_run('第 ')
    style_run(r, 8.5, color='666666')
    fldChar1 = OxmlElement('w:fldChar')
    fldChar1.set(qn('w:fldCharType'), 'begin')
    instrText = OxmlElement('w:instrText')
    instrText.set(qn('xml:space'), 'preserve')
    instrText.text = 'PAGE'
    fldChar2 = OxmlElement('w:fldChar')
    fldChar2.set(qn('w:fldCharType'), 'end')
    r._r.append(fldChar1); r._r.append(instrText); r._r.append(fldChar2)
    add_text(paragraph, ' 页', 8.5, color='666666')

doc = Document()
section = doc.sections[0]
section.top_margin = Cm(2.1)
section.bottom_margin = Cm(1.8)
section.left_margin = Cm(2.15)
section.right_margin = Cm(2.15)

styles = doc.styles
styles['Normal'].font.name = 'Microsoft YaHei'
styles['Normal']._element.rPr.rFonts.set(qn('w:eastAsia'), 'Microsoft YaHei')
styles['Normal'].font.size = Pt(10.5)
for name, size in [('Title', 24), ('Heading 1', 15), ('Heading 2', 12)]:
    style = styles[name]
    style.font.name = 'Microsoft YaHei'
    style._element.rPr.rFonts.set(qn('w:eastAsia'), 'Microsoft YaHei')
    style.font.size = Pt(size)
    style.font.color.rgb = RGBColor(0, 0, 0)

# Header and footer
header = section.header
hp = header.paragraphs[0]
hp.alignment = WD_ALIGN_PARAGRAPH.RIGHT
add_text(hp, 'Mesh 便携指挥箱 WireGuard 远程接入实施方案', 8.5, color='666666')
footer = section.footer
add_page_number(footer.paragraphs[0])

# Cover
p = doc.add_paragraph(style='Title')
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
p.paragraph_format.space_before = Pt(88)
p.paragraph_format.space_after = Pt(15)
r = p.add_run('Mesh 便携指挥箱')
style_run(r, 25, bold=True, color='000000')
p2 = doc.add_paragraph()
p2.alignment = WD_ALIGN_PARAGRAPH.CENTER
p2.paragraph_format.space_after = Pt(30)
add_text(p2, 'WireGuard 远程接入实施方案', 18, bold=True, color=NAVY)
intro = doc.add_paragraph()
intro.alignment = WD_ALIGN_PARAGRAPH.CENTER
intro.paragraph_format.space_after = Pt(12)
add_text(intro, '适用于现场 SIM 公网接入  办公室远程业务操作  实时视频查看  Windows 运维', 10.5, color='4B5563')
add_para(doc, '', after=16)
add_table(doc, ['项目', '内容'], [
    ('方案目标', '办公室人员通过安全专网访问现场指挥箱系统、实时画面和 Windows 桌面，不向互联网暴露业务端口。'),
    ('推荐架构', '公司机房或云端固定公网 IP 的 WireGuard 汇聚网关；现场指挥箱和办公室电脑均主动接入。'),
    ('适用对象', 'Windows 便携指挥箱、Mesh 手持式电台、SIM 卡移动网络、Docker 化 SSK 业务系统。'),
    ('版本', 'V1.0'),
    ('日期', '2026 年 11 月 9 日'),
], widths=[3.2, 11.2], font_size=9.5)
add_para(doc, '本方案的结论是：现场端不需要公网固定 IP，也不需要运营商开端口。只要 SIM 卡能稳定出网，WireGuard 隧道即可主动建立并保持；办公室电脑加入同一专网后，通过受控路由访问指挥箱的 Mesh 局域网地址。', before=8, after=0, first=0.74)
doc.add_page_break()

# Contents
heading(doc, '目录', 1)
toc = [
    '1 方案目标和边界', '2 总体架构和访问流程', '3 地址 域名 端口规划', '4 实施前准备',
    '5 云端 WireGuard 汇聚网关部署', '6 现场 Windows 指挥箱配置', '7 办公室电脑配置',
    '8 SSK 系统和 WebRTC 视频适配', '9 安全加固要求', '10 上线验收和运维', '11 故障排查', '附录 A 部署参数表', '附录 B 配置模板'
]
for item in toc:
    p = add_para(doc, item, before=0, after=4, indent=0.3, size=11)
doc.add_page_break()

heading(doc, '1 方案目标和边界', 1)
add_para(doc, '本方案用于让后方办公室在不改变现场 Mesh 组网方式的前提下，安全地查看和操作便携指挥箱上的有限空间作业安全管控系统。办公室用户可以在浏览器中查看大屏、告警、设备状态、实时视频和录像；授权运维人员可以在 VPN 内使用 Windows 远程桌面维护指挥箱。', first=0.74)
add_para(doc, '现场 SIM 卡通常使用运营商 CGNAT 地址，办公室无法从公网主动连接到指挥箱。因此方案采用“现场端主动出站连接”的方式：指挥箱建立到固定公网 WireGuard 网关的加密隧道，办公室电脑也连接同一网关，由网关转发专网内的访问流量。', first=0.74)
heading(doc, '1.1 范围', 2)
add_table(doc, ['范围', '纳入内容', '不纳入内容'], [
    ('网络接入', 'SIM 出网、WireGuard 隧道、办公室到指挥箱的受控路由', '更换运营商、Mesh 电台射频参数优化'),
    ('业务访问', '系统 HTTPS 页面、WebRTC 实时视频、录像下载、业务操作', '将业务数据同步到云端的长期数据平台'),
    ('设备运维', 'VPN 内 RDP、Docker 服务检查、日志获取', '无人值守硬件维修和 BIOS 级远程控制'),
], widths=[2.6, 6.0, 6.0])
heading(doc, '1.2 设计原则', 2)
for t in ['只允许出站建立 VPN 隧道，SIM 公网侧不开放任何业务入站端口。', '办公室访问采用最小路由：仅访问每台指挥箱的指定 Mesh 主机地址，不默认开放整个现场网段。', '实时视频继续由现场 ZLMediaKit 提供；WireGuard 只提供受控传输路径，不改变安全帽与 Mesh 电台的本地通信关系。', 'VPN 身份、系统账号和 Windows 账号分离管理；撤销任一账号不影响其他层。']:
    add_bullet(doc, t)

heading(doc, '2 总体架构和访问流程', 1)
add_para(doc, '推荐使用一台部署在公司机房或云平台的 Linux 主机作为汇聚网关。该主机必须具有固定公网 IPv4 地址或可解析的公网域名，仅开放 WireGuard 的 UDP 监听端口。现场指挥箱与办公室电脑均作为 WireGuard 客户端。', first=0.74)
add_table(doc, ['办公室网络', '汇聚网关', '现场指挥箱', 'Mesh 手持电台'], [
    ('办公室电脑\n10.77.0.10', '固定公网 IP\nWireGuard 10.77.0.1', 'WireGuard 10.77.0.21\nMesh IP 192.168.144.52', '仅保留现场 Mesh 通信\n不直接暴露到办公室'),
], widths=[3.55, 3.55, 4.15, 3.35], font_size=8.6)
add_para(doc, '访问路径：办公室浏览器请求 ssk field01 example com，办公室 DNS 将该名称解析为 192.168.144.52；目标报文由 WireGuard 送达汇聚网关，再送至现场指挥箱。指挥箱上的 nginx 返回系统页面并反向代理业务接口；WebRTC 信令走 HTTPS 3000 端口，媒体流走 ZLMediaKit 的 8000 UDP 端口。', first=0.74)
heading(doc, '2.1 业务流量说明', 2)
add_table(doc, ['业务', '办公室访问目标', '协议端口', '说明'], [
    ('系统业务页面', '192.168.144.52', 'TCP 3000', 'nginx HTTPS 入口，承载前端、API、告警和设备控制。'),
    ('实时视频信令', '192.168.144.52', 'TCP 3000', '浏览器通过 /zlm 与 ZLMediaKit 协商 WebRTC。'),
    ('实时视频媒体', '192.168.144.52', 'UDP 8000，必要时 TCP 8000', '浏览器与 ZLMediaKit 建立音视频传输。'),
    ('Windows 运维', '指挥箱 VPN 地址 10.77.0.21', 'TCP 3389', '仅授权运维人员使用；必须开启 NLA。'),
], widths=[3.1, 3.2, 3.0, 5.3])
add_para(doc, '注意：不需要为办公室开放 RTMP 1935、后端 API 9991、caps 5010、数据库或 Redis 端口。办公室的日常业务操作只使用 HTTPS 3000 和 WebRTC 8000。', before=4, after=4)

heading(doc, '3 地址 域名 端口规划', 1)
add_para(doc, '以下地址为示例。正式上线前需填入附录 A，并确认办公室现网、现场 Mesh 网段和其他指挥箱网段互不重叠。多台指挥箱不得都使用相同的 192.168.144.0/24 网段，否则汇聚网关无法准确选择转发路径。', first=0.74)
add_table(doc, ['对象', '示例值', '用途和约束'], [
    ('WireGuard 汇聚网关', '10.77.0.1/24\nvpn.example.com', '固定公网 IP；监听 UDP 51820；开启 IPv4 转发。'),
    ('办公室电脑', '10.77.0.10/32', '每位用户或每台办公终端单独分配地址与密钥。'),
    ('现场指挥箱 Field01', '10.77.0.21/32', 'Windows WireGuard 接口地址；通过 PersistentKeepalive 保持隧道。'),
    ('指挥箱 Mesh 地址', '192.168.144.52/32', '办公室访问系统和 WebRTC 的目标地址。'),
    ('Field01 访问域名', 'ssk-field01.example.com', '办公室分离 DNS 解析到 192.168.144.52；证书须覆盖该域名。'),
], widths=[3.0, 4.2, 7.4])
heading(doc, '3.1 路由设计', 2)
add_para(doc, '汇聚网关中，Field01 对端的 AllowedIPs 同时包含 10.77.0.21/32 和 192.168.144.52/32。这样当办公室客户端访问 192.168.144.52 时，网关会将该流量送入 Field01 隧道。办公室客户端仅路由 10.77.0.0/24 与 192.168.144.52/32 到 VPN；这比下发整个 192.168.144.0/24 更符合最小暴露原则。', first=0.74)
add_para(doc, '若未来确实需要从办公室直接访问其他现场 Mesh 设备，应为各现场分配独立网段，并在指挥箱上补充 IP 转发和回程路由或受控 NAT。本方案默认不做这一扩展，避免无意暴露手持电台管理面。', first=0.74)

heading(doc, '4 实施前准备', 1)
add_table(doc, ['项', '责任方', '完成标准'], [
    ('公网网关', '信息化或云平台管理员', 'Linux 主机具有固定公网 IP、可创建安全组规则、可长期运行。'),
    ('SIM 卡与现场网络', '现场实施人员', '指挥箱可稳定访问公网，确认 UDP 51820 出站未被运营商策略阻断。'),
    ('Mesh 地址确认', '现场实施人员', '记录指挥箱实际 Mesh IPv4；确认其不是办公室网络的重叠网段。'),
    ('域名和证书', '信息化或平台管理员', '为每台指挥箱准备访问域名、DNS 解析和有效 HTTPS 证书。'),
    ('授权清单', '项目负责人', '明确可使用 VPN、系统账号和 RDP 的人员，建立离场或离岗撤权流程。'),
], widths=[3.0, 3.8, 7.8])
heading(doc, '4.1 带宽和资费评估', 2)
add_para(doc, '视频上行消耗由实际编码码率决定。建议按每路 1080P H264 实时视频 2 到 4 Mbps、每路 720P 约 1 到 2 Mbps 估算，并预留至少 30% 波动空间。多路视频连续观看会显著消耗 SIM 流量，应在系统侧限制并发观看路数，办公室侧优先按需打开视频窗口。', first=0.74)
add_para(doc, '网关只承担加密转发，CPU 压力通常较低；实际瓶颈通常是现场 SIM 上行带宽、移动网络抖动和数据套餐。上线前应在现场连续测试不少于 30 分钟。', first=0.74)

heading(doc, '5 云端 WireGuard 汇聚网关部署', 1)
add_para(doc, '以下示例以 Ubuntu 22.04 或 24.04 LTS 为例。网关仅暴露 UDP 51820；SSH 管理端口应限制为公司固定出口 IP 或堡垒机来源。密钥必须在受管控终端生成和保存，以下文本中的密钥占位符不可直接使用。', first=0.74)
heading(doc, '5.1 安装和系统转发', 2)
add_code(doc, '''sudo apt update
sudo apt install -y wireguard ufw
sudo sysctl -w net.ipv4.ip_forward=1
echo 'net.ipv4.ip_forward=1' | sudo tee /etc/sysctl.d/99-wireguard-forward.conf
sudo sysctl --system
sudo ufw allow 51820/udp
sudo ufw enable''')
heading(doc, '5.2 生成密钥', 2)
add_code(doc, '''umask 077
wg genkey | tee /etc/wireguard/server.key | wg pubkey > /etc/wireguard/server.pub
# 每个现场指挥箱和每台办公室电脑分别生成一对独立密钥
wg genkey | tee field01.key | wg pubkey > field01.pub
wg genkey | tee office01.key | wg pubkey > office01.pub''')
add_para(doc, '私钥不得通过微信、邮件正文、工单评论或源代码仓库传递。可使用企业密码库、受控加密文件或线下导入方式交付。', before=0, after=5)
heading(doc, '5.3 网关配置文件', 2)
add_para(doc, '创建 /etc/wireguard/wg0.conf，并将尖括号内容替换为真实密钥。PostUp 和 PostDown 规则仅允许 WireGuard 接口之间转发，不执行互联网出口 NAT。', first=0.74)
add_code(doc, '''[Interface]
Address = 10.77.0.1/24
ListenPort = 51820
PrivateKey = <SERVER_PRIVATE_KEY>
PostUp = iptables -A FORWARD -i %i -o %i -j ACCEPT
PostDown = iptables -D FORWARD -i %i -o %i -j ACCEPT

# Field01 portable command box
[Peer]
PublicKey = <FIELD01_PUBLIC_KEY>
AllowedIPs = 10.77.0.21/32, 192.168.144.52/32

# Office computer of authorized operator
[Peer]
PublicKey = <OFFICE01_PUBLIC_KEY>
AllowedIPs = 10.77.0.10/32''')
add_code(doc, '''sudo chmod 600 /etc/wireguard/wg0.conf
sudo systemctl enable --now wg-quick@wg0
sudo wg show
sudo ss -lunp | grep 51820''')
add_para(doc, '云平台安全组还必须允许 UDP 51820 入站。若公司机房网关位于防火墙后，需要把公网 UDP 51820 仅映射到该网关。', first=0.74)

heading(doc, '6 现场 Windows 指挥箱配置', 1)
heading(doc, '6.1 WireGuard 客户端配置', 2)
add_para(doc, '在指挥箱安装官方 WireGuard for Windows。导入以下配置后先手工激活验证；验证通过后安装为隧道服务，使其在无人登录或断线重拨后仍能恢复。实际配置中的私钥只能保存在现场设备。', first=0.74)
add_code(doc, '''[Interface]
PrivateKey = <FIELD01_PRIVATE_KEY>
Address = 10.77.0.21/32

[Peer]
PublicKey = <SERVER_PUBLIC_KEY>
Endpoint = vpn.example.com:51820
AllowedIPs = 10.77.0.0/24
PersistentKeepalive = 25''')
add_para(doc, r'在 WireGuard 图形界面中导入并激活后，确认“最新握手”持续更新。将确认可用的配置文件保存到受控路径，例如 C:\ProgramData\WireGuard\field01.conf，再以管理员 PowerShell 执行以下命令安装服务。', first=0.74)
add_code(doc, r'''& 'C:\Program Files\WireGuard\wireguard.exe' /installtunnelservice C:\ProgramData\WireGuard\field01.conf
Get-Service 'WireGuardTunnel$field01'
wg show''')
add_para(doc, '若需停止或移除该服务，应在变更窗口中执行 /uninstalltunnelservice field01。不要通过结束进程的方式停止 WireGuard。', first=0.74)
heading(doc, '6.2 Windows 防火墙规则', 2)
add_para(doc, '以下示例允许来自 VPN 网段的系统 HTTPS、WebRTC 和 RDP 访问，同时保留现场 Mesh 网段对视频与设备服务的必要访问。实际端口名称须结合现有防火墙策略核对，创建规则前先导出或记录原策略。', first=0.74)
add_code(doc, '''$vpn = '10.77.0.0/24'
$mesh = '192.168.144.0/24'
New-NetFirewallRule -DisplayName 'SSK VPN HTTPS 3000' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 3000 -RemoteAddress $vpn
New-NetFirewallRule -DisplayName 'SSK VPN WebRTC UDP 8000' -Direction Inbound -Action Allow -Protocol UDP -LocalPort 8000 -RemoteAddress $vpn,$mesh
New-NetFirewallRule -DisplayName 'SSK VPN WebRTC TCP 8000' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8000 -RemoteAddress $vpn,$mesh
New-NetFirewallRule -DisplayName 'SSK VPN RDP' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 3389 -RemoteAddress $vpn''')
add_para(doc, '防火墙中不要创建“任意来源允许 3000、8000 或 3389”的规则。若 SIM 获得了可路由公网 IPv4，这一限制尤为关键。', before=0, after=5)
heading(doc, '6.3 Docker 服务暴露面收缩', 2)
add_para(doc, '当前部署编排包含 MySQL、MongoDB、Redis、caps、netcore 和 ZLMediaKit 的宿主机端口映射。日常业务只需要 nginx HTTPS 入口；数据库容器之间通过 Docker 内部网络互通。因此生产部署应取消不必要的数据库宿主机端口映射，或至少在 Windows 防火墙中禁止非本机、非现场 Mesh、非 VPN 网段访问。更改 docker compose 前需在维护窗口备份并验证恢复路径。', first=0.74)

heading(doc, '7 办公室电脑配置', 1)
add_para(doc, '每位办公室用户或每台固定办公终端使用独立 WireGuard 密钥和独立 VPN 地址。不得多人共用一个配置文件；这样可单独撤销人员权限并保留准确审计记录。', first=0.74)
heading(doc, '7.1 办公室客户端配置模板', 2)
add_code(doc, '''[Interface]
PrivateKey = <OFFICE01_PRIVATE_KEY>
Address = 10.77.0.10/32
DNS = <CORPORATE_DNS_OR_SPLIT_DNS_SERVER>

[Peer]
PublicKey = <SERVER_PUBLIC_KEY>
Endpoint = vpn.example.com:51820
AllowedIPs = 10.77.0.0/24, 192.168.144.52/32
PersistentKeepalive = 25''')
heading(doc, '7.2 域名和证书', 2)
add_para(doc, '办公室浏览器应使用域名访问，而不是直接使用 IP 地址，以避免 HTTPS 证书名称不匹配。建议在公司内部 DNS 或办公终端的受控 hosts 策略中创建记录：ssk-field01.example.com 指向 192.168.144.52。nginx 证书必须包含该域名；可使用企业内部 CA，或使用公有域名的 DNS 验证方式签发证书。', first=0.74)
add_para(doc, '访问地址示例： https://ssk-field01.example.com:3000/#/board 。系统页面加载后，前端会同源访问 /api、/caps、/websocket 和 /zlm，不需要在浏览器中配置额外 API 地址。', first=0.74)
heading(doc, '7.3 Windows 远程桌面', 2)
add_para(doc, '如需操作指挥箱 Windows 桌面，办公室人员先连接 WireGuard，再使用 mstsc 访问 10.77.0.21。目标 Windows 需为支持远程桌面的版本，启用“仅允许运行使用网络级别身份验证的远程桌面的计算机连接”，并使用独立的运维账号。远程桌面不是业务用户的常规访问方式。', first=0.74)

heading(doc, '8 SSK 系统和 WebRTC 视频适配', 1)
add_para(doc, '现场安全帽与手持电台仍通过 Mesh 局域网连接指挥箱。VPN 只解决办公室到指挥箱的访问，不应将设备侧推流地址改为公网地址或 VPN 地址。办公室通过 WireGuard 路由访问同一个 Mesh IP，因此可复用现场地址体系。', first=0.74)
heading(doc, '8.1 必须核对的现有配置', 2)
add_table(doc, ['文件或位置', '配置项', '要求'], [
    (' .env.main ', 'RTMP_URL', '填写指挥箱实际 Mesh IP，供现场设备向 ZLMediaKit 推流使用。'),
    (' .env.main ', 'WEBRTC_URL', '填写 https://指挥箱实际MeshIP:3000/zlm，供设备侧 WebRTC 地址生成使用。'),
    (' zlm conf config ini ', 'rtc.externIP', '必须填写同一个、办公室可通过 WireGuard 路由到的指挥箱 Mesh IP。'),
    (' nginx nginx conf ', 'server_name 和证书路径', '加入每台指挥箱的访问域名，证书应匹配办公室使用的域名。'),
], widths=[3.4, 3.2, 8.2])
add_para(doc, '现有部署文件中 .env.main 的 RTMP_URL 和 WEBRTC_URL 与 ZLMediaKit 的 rtc.externIP 可能不是同一地址。上线前必须通过 ipconfig 确认指挥箱实际 Mesh IP，并统一上述配置。地址不一致会导致浏览器能打开大屏但实时视频黑屏，或现场设备无法推流。', first=0.74)
heading(doc, '8.2 推荐修改示例', 2)
add_code(doc, '''# .env.main
RTMP_URL=192.168.144.52
WEBRTC_URL=https://192.168.144.52:3000/zlm

# zlm/conf/config.ini
[rtc]
externIP=192.168.144.52''')
add_para(doc, '修改环境变量后使用 docker compose up -d 重建受影响容器；只修改 zlm 配置时按现有部署规范重启或重建 zlmediakit。修改前应备份文件并记录变更时间。', first=0.74)
heading(doc, '8.3 WebRTC 验证逻辑', 2)
add_para(doc, '浏览器首先经 TCP 3000 的 /zlm 路径完成 SDP 协商，然后根据 ZLMediaKit 返回的 rtc.externIP 建立 UDP 8000 媒体通道。因而仅能打开系统页面并不代表视频链路成功。办公室 VPN 客户端必须同时具备到 192.168.144.52 的 TCP 3000 和 UDP 8000 路由与防火墙权限。', first=0.74)

heading(doc, '9 安全加固要求', 1)
add_table(doc, ['控制点', '实施要求', '验收证据'], [
    ('VPN 密钥', '每端独立密钥；人员离岗、设备遗失或疑似泄露时立即从网关删除对应 Peer 并重新签发。', 'wg show 输出与密钥台账一致。'),
    ('业务账号', '取消默认账号与弱口令；管理员账号按人分配，设置合理密码与锁定策略。', '账号清单、抽查登录策略。'),
    ('数据库与缓存', '不向 SIM 公网开放；优先移除 compose 宿主机映射。', '端口扫描和防火墙规则截图。'),
    ('Windows RDP', '仅允许 VPN 网段；开启 NLA；使用独立运维账号。', '防火墙规则和远程桌面设置。'),
    ('日志和审计', '保留网关系统日志、WireGuard 配置变更记录、业务操作日志。', '日志留存目录和变更记录。'),
    ('补丁', 'Windows、Docker、WireGuard 和网关系统按维护计划更新。', '版本清单和维护记录。'),
], widths=[2.5, 7.2, 4.8])
add_para(doc, '严禁将 WireGuard 私钥、数据库密码、Redis 密码、ZLMediaKit API 密钥或 CompreFace API Key 提交到 Git 仓库、截图群聊或嵌入本方案。现有部署若仍保留默认数据库或缓存密码，应在上线前完成轮换并更新相关环境变量。', first=0.74)

heading(doc, '10 上线验收和运维', 1)
heading(doc, '10.1 上线顺序', 2)
for t in ['在云端部署 WireGuard 网关，确认 UDP 51820 从公网可达。', '现场指挥箱接入 SIM 网络，导入 WireGuard 配置，确认最新握手可持续更新。', '办公室电脑接入 WireGuard，验证可 ping 10.77.0.21 和 192.168.144.52。', '在办公室浏览器访问系统域名，验证登录、告警、设备控制、实时视频和录像回放。', '按权限测试 RDP，并确认非 VPN 网络无法访问 RDP、3000 和 8000。', '记录地址、密钥编号、配置版本、测试人和测试结果，完成交接。']:
    add_number(doc, t)
heading(doc, '10.2 验收表', 2)
add_table(doc, ['编号', '验收项', '操作与预期结果', '结果'], [
    ('A01', '隧道存活', '现场和办公室 wg show 均有持续更新的 latest handshake。', '□通过 □失败'),
    ('A02', '系统访问', '浏览器打开系统域名，无证书名称告警，可正常登录。', '□通过 □失败'),
    ('A03', '实时视频', '从办公室打开至少一路现场视频，连续观看 30 分钟无持续黑屏。', '□通过 □失败'),
    ('A04', '业务操作', '授权用户可执行设备控制、查询及录像回放，结果正确。', '□通过 □失败'),
    ('A05', '远程桌面', '授权账号可从 VPN 内访问 10.77.0.21；断开 VPN 后连接失败。', '□通过 □失败'),
    ('A06', '公网暴露', '非 VPN 网络对业务端口、数据库端口和 RDP 扫描均不可达。', '□通过 □失败'),
], widths=[1.3, 2.7, 8.2, 2.3])
heading(doc, '10.3 日常运维', 2)
add_table(doc, ['频率', '检查项', '处理动作'], [
    ('每日或任务前', 'WireGuard 握手、SIM 信号、Docker 容器状态', '发现异常先确认 SIM 出网和 wg show，再检查业务容器。'),
    ('每月', '账户权限、密钥台账、证书有效期、系统补丁', '撤销无效账号；更新证书和补丁。'),
    ('每次变更后', '访问、视频、RDP 和公网暴露复测', '记录变更版本、回退点和验收结果。'),
], widths=[3.0, 6.2, 5.3])

heading(doc, '11 故障排查', 1)
add_table(doc, ['现象', '优先检查', '处理方向'], [
    ('latest handshake 为空', 'SIM 是否出网；DNS 是否可解析 vpn.example.com；云端 UDP 51820 安全组', '确认指挥箱到公网 UDP 出站；检查网关服务 wg-quick@wg0。'),
    ('可握手但访问不了系统', '网关 AllowedIPs 是否包含 192.168.144.52/32；办公室 AllowedIPs；Windows 防火墙', '按第 3.1 节核对最小路由；测试 ping 和 Test-NetConnection。'),
    ('系统页面能打开 视频黑屏', 'UDP 8000 路由与防火墙；rtc.externIP 是否为实际 Mesh IP', '核对浏览器到 192.168.144.52:8000 的可达性与 ZLM 日志。'),
    ('证书告警', '浏览器访问的域名与 nginx 证书 SAN；分离 DNS 记录', '使用系统域名访问，不直接使用 IP；更新证书和 nginx server_name。'),
    ('RDP 连接失败', '目标 Windows 版本、远程桌面开关、NLA、VPN 网段防火墙规则', '先用 VPN IP 测试 3389，再检查授权用户组。'),
    ('现场设备断流', '设备到 Mesh IP 的 RTMP 或 WebRTC 地址、ZLMediaKit 容器和本地网络', '不要将设备侧地址改成 VPN 地址；按现场推流链路排查。'),
], widths=[3.0, 6.3, 5.2])
add_code(doc, '''# 办公室 Windows 常用验证命令
wg show
ping 10.77.0.21
ping 192.168.144.52
Test-NetConnection 192.168.144.52 -Port 3000
Test-NetConnection 10.77.0.21 -Port 3389

# 云端网关常用验证命令
sudo wg show
sudo systemctl status wg-quick@wg0
sudo journalctl -u wg-quick@wg0 --since '30 minutes ago' ''')

heading(doc, '附录 A 部署参数表', 1)
add_para(doc, '下表由实施人员在部署前填写。所有私钥字段只登记密码库引用编号，不在 Word 文档中填写明文。', first=0.74)
add_table(doc, ['参数', 'Field01 填写值', '备注'], [
    ('WireGuard 网关公网域名', '____________________________', '例如 vpn.example.com'),
    ('WireGuard 网关公网 IPv4', '____________________________', '固定 IP 或防火墙映射地址'),
    ('WireGuard 监听端口', '____________________________', '默认 51820 UDP'),
    ('指挥箱 VPN 地址', '____________________________', '例如 10.77.0.21/32'),
    ('办公室客户端 VPN 地址', '____________________________', '每台终端不同'),
    ('指挥箱实际 Mesh IP', '____________________________', '与 RTMP_URL、WEBRTC_URL、rtc.externIP 对齐'),
    ('现场 Mesh 网段', '____________________________', '确认不与办公室或其他现场重叠'),
    ('系统访问域名', '____________________________', '例如 ssk-field01.example.com'),
    ('TLS 证书到期日', '____________________________', '提前 30 天续期'),
    ('密钥库记录编号', '____________________________', '不填写私钥明文'),
], widths=[4.5, 5.4, 4.6])

heading(doc, '附录 B 配置模板', 1)
add_para(doc, '以下模板仅用于复制后替换占位符。执行前应由双人复核地址、AllowedIPs、域名和端口。', first=0.74)
heading(doc, 'B 1 Field01 路由关系', 2)
add_table(doc, ['位置', '指向 192.168.144.52 的处理'], [
    ('办公室 WireGuard 客户端', 'AllowedIPs 包含 192.168.144.52/32，将系统访问请求加密送到汇聚网关。'),
    ('云端汇聚网关', 'Field01 Peer 的 AllowedIPs 包含 192.168.144.52/32，将请求转入 Field01 隧道。'),
    ('现场 Windows 指挥箱', '该地址是自身 Mesh 网卡地址，nginx 和 ZLMediaKit 在本机响应并通过 WireGuard 回包。'),
], widths=[4.2, 10.3])
heading(doc, 'B 2 变更和回退', 2)
add_para(doc, '每次修改 WireGuard、Windows 防火墙、Docker 端口或 ZLMediaKit 外部 IP 前，先导出现有配置、记录当前容器状态并准备回退命令。若远程变更失败，优先通过现场人员恢复原 WireGuard 配置或防火墙规则；不要在现场业务时段直接删除全部网络配置。', first=0.74)

# Core metadata
props = doc.core_properties
props.title = 'Mesh 便携指挥箱 WireGuard 远程接入实施方案'
props.subject = '现场 SIM 网络下的办公室远程访问与视频接入方案'
props.author = 'SSK 项目组'
props.keywords = 'WireGuard, Mesh, 便携指挥箱, WebRTC, 远程接入'

OUT.parent.mkdir(parents=True, exist_ok=True)
doc.save(OUT)
print(OUT)
